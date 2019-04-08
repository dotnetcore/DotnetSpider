using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Downloader.Entity;
using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器代理
	/// </summary>
	public abstract class DownloaderAgentBase : IDownloaderAgent
	{
		private bool _isRunning;

		private readonly IMessageQueue _mq;
		private readonly IDownloaderAllocator _downloaderAllocator;
		private readonly IDownloaderAgentOptions _options;
		private const string DownloaderPersistFolderName = "downloaders";

		private readonly ConcurrentDictionary<string, IDownloader> _cache =
			new ConcurrentDictionary<string, IDownloader>();

		/// <summary>
		/// 日志接口
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// 配置下载器
		/// </summary>
		protected Action<IDownloader> ConfigureDownloader { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">下载器代理选项</param>
		/// <param name="mq">消息队列</param>
		/// <param name="downloaderAllocator">分配下载器的接口</param>
		/// <param name="networkCenter">网络中心</param>
		/// <param name="logger">日志接口</param>
		protected DownloaderAgentBase(
			IDownloaderAgentOptions options,
			IMessageQueue mq,
			IDownloaderAllocator downloaderAllocator,
			NetworkCenter networkCenter,
			ILogger logger)
		{
			_mq = mq;
			_downloaderAllocator = downloaderAllocator;
			_options = options;
			Framework.NetworkCenter = networkCenter;
			Logger = logger;
		}

		static DownloaderAgentBase()
		{
			var downloaderPersistDirectory =
				new DirectoryInfo(Path.Combine(Framework.BaseDirectory, DownloaderPersistFolderName));
			if (!downloaderPersistDirectory.Exists)
			{
				downloaderPersistDirectory.Create();
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (_isRunning)
			{
				throw new SpiderException($"下载器代理 {_options.AgentId} 正在运行中");
			}

			_isRunning = true;

			// 加载并初始化未释放的下载器
			await LoadDownloaderAsync();

			// 注册节点
			var json = JsonConvert.SerializeObject(new DownloaderAgent
			{
				Id = _options.AgentId,
				Name = _options.Name,
				ProcessorCount = Environment.ProcessorCount,
				TotalMemory = Framework.TotalMemory,
				CreationTime = DateTime.Now,
				LastModificationTime = DateTime.Now
			});

			await _mq.PublishAsync(Framework.DownloaderCenterTopic, $"|{Framework.RegisterCommand}|{json}");

			// 订阅节点
			SubscribeMessage();

			// 开始心跳
			HeartbeatAsync().ConfigureAwait(false).GetAwaiter();

			// 循环清理过期下载器
			ReleaseDownloaderAsync().ConfigureAwait(false).GetAwaiter();

			Logger.LogInformation($"下载器代理 {_options.AgentId} 启动完毕");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_mq.Unsubscribe(_options.AgentId);
			_isRunning = false;
			Logger.LogInformation($"下载器代理 {_options.AgentId} 退出");
#if NETFRAMEWORK
			return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		private Task HeartbeatAsync()
		{
			return Task.Factory.StartNew(async () =>
			{
				while (_isRunning)
				{
					Thread.Sleep(5000);
					try
					{
						var json = JsonConvert.SerializeObject(new DownloaderAgentHeartbeat
						{
							AgentId = _options.AgentId,
							AgentName = _options.Name,
							FreeMemory = (int) Framework.GetFreeMemory(),
							DownloaderCount = _cache.Count,
							CreationTime = DateTime.Now
						});

						await _mq.PublishAsync(Framework.DownloaderCenterTopic,
							$"|{Framework.HeartbeatCommand}|{json}");
						Logger.LogDebug($"下载器代理 {_options.AgentId} 发送心跳成功");
					}
					catch (Exception e)
					{
						Logger.LogDebug($"下载器代理 {_options.AgentId} 发送心跳失败: {e}");
					}
				}
			});
		}

		private Task DownloadAsync(string message)
		{
			var requests = JsonConvert.DeserializeObject<Request[]>(message);

			if (requests.Length > 0)
			{
				// 超时 60 秒的不再下载 
				// 下载中心下载请求批量传送，因此反序列化的请求需要按拥有者标号分组。
				// 对于同一个任务应该是顺序下载。TODO: 因为是使用多线程，是否此时保证顺序并不会启作用？
				var groupings = requests.Where(x => (DateTime.Now - x.CreationTime).TotalSeconds < 60)
					.GroupBy(x => x.OwnerId).ToDictionary(x => x.Key, y => y.ToList());
				foreach (var grouping in groupings)
				{
					foreach (var request in grouping.Value)
					{
						Task.Factory.StartNew(async () =>
						{
							var response = await DownloadAsync(request);
							if (response != null)
							{
								await _mq.PublishAsync($"{Framework.ResponseHandlerTopic}{grouping.Key}",
									JsonConvert.SerializeObject(new[] {response}));
							}
						}).ConfigureAwait(false).GetAwaiter();
					}
				}
			}
			else
			{
				Logger.LogWarning("下载请求数: 0");
			}

#if NETFRAMEWORK
			return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		private async Task<Response> DownloadAsync(Request request)
		{
			if (_cache.TryGetValue(request.OwnerId, out IDownloader downloader))
			{
				var response = await downloader.DownloadAsync(request);
				return response;
			}

			Logger.LogError($"未找到任务 {request.OwnerId} 的下载器");
			return new Response
			{
				Request = request,
				Exception = "任务下载器丢失",
				Success = false,
				AgentId = _options.AgentId
			};
		}

		private async Task LoadDownloaderAsync()
		{
			try
			{
				var downloaderFiles =
					Directory.GetFiles(Path.Combine(Framework.BaseDirectory, DownloaderPersistFolderName))
						.Where(x => !Path.GetFileName(x).StartsWith("."));
				foreach (var downloaderFile in downloaderFiles)
				{
					await AllotDownloaderAsync(File.ReadAllText(downloaderFile), true);
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"下载器代理从文件加载失败: {e}");
			}
		}

		private Task ReleaseDownloaderAsync()
		{
			return Task.Factory.StartNew(() =>
			{
				while (_isRunning)
				{
					Thread.Sleep(1000);

					try
					{
						var now = DateTime.Now;
						var expires = new List<string>();
						foreach (var kv in _cache)
						{
							var downloader = kv.Value;
							if ((now - downloader.LastUsedTime).TotalSeconds > 600)
							{
								downloader.Dispose();
								expires.Add(kv.Key);
							}
						}

						foreach (var expire in expires)
						{
							File.Delete(Path.Combine(Framework.BaseDirectory, DownloaderPersistFolderName, expire));

							if (!_cache.TryRemove(expire, out _))
							{
								Logger.LogWarning($"下载器代理 {_options.AgentId} 释放过期下载器 {expire} 失败");
							}
						}

						var msg = $"下载器代理 {_options.AgentId} 释放过期下载器: {expires.Count}";
						if (expires.Count > 0)
						{
							Logger.LogInformation(msg);
						}
						else
						{
							// Logger.LogDebug(msg);
						}
					}
					catch (Exception e)
					{
						Logger.LogError($"下载器代理 {_options.AgentId} 释放过期下载器失败: {e}");
					}
				}
			});
		}

		/// <summary>
		/// 订阅消息
		/// </summary>
		private void SubscribeMessage()
		{
			while (true)
			{
				try
				{
					_mq.Subscribe(_options.AgentId, HandleMessage);
					Logger.LogInformation($"订阅节点 {_options.AgentId} 推送请求成功");
					return;
				}
				catch (Exception e)
				{
					Logger.LogError($"订阅节点 {_options.AgentId} 推送请求结果失败: {e.Message}");
					Thread.Sleep(1000);
				}
			}
		}

		private async Task HandleMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				Logger.LogWarning($"下载器代理 {_options.AgentId} 接收到空消息");
				return;
			}
#if DEBUG

			Logger.LogDebug($"下载器代理 {_options.AgentId} 接收到消息: {message}");
#endif

			try
			{
				var commandMessage = message.ToCommandMessage();

				if (commandMessage == null)
				{
					Logger.LogWarning($"下载器代理 {_options.AgentId} 接收到非法消息: {message}");
					return;
				}

				switch (commandMessage.Command)
				{
					case Framework.AllocateDownloaderCommand:
					{
						await AllotDownloaderAsync(commandMessage.Message);
						break;
					}
					case Framework.DownloadCommand:
					{
						await DownloadAsync(commandMessage.Message);
						break;
					}
					case Framework.ExitCommand:
					{
						await StopAsync(default);
						break;
					}
					default:
					{
						Logger.LogError($"下载器代理 {_options.AgentId} 无法处理消息: {message}");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"下载器代理 {_options.AgentId} 处理消息: {message} 失败, 异常: {e}");
			}
		}

		/// <summary>
		/// 分配下载器
		/// </summary>
		/// <returns></returns>
		private async Task AllotDownloaderAsync(string message, bool reAllot = false)
		{
			var allotDownloaderMessage = JsonConvert.DeserializeObject<AllocateDownloaderMessage>(message);
			if (allotDownloaderMessage == null)
			{
				Logger.LogError($"无法分配下载器，消息不正确: {message}");
				return;
			}

			if (reAllot)
			{
				allotDownloaderMessage.CreationTime = DateTime.Now;
			}

			if ((DateTime.Now - allotDownloaderMessage.CreationTime).TotalSeconds < 30)
			{
				if (!_cache.ContainsKey(allotDownloaderMessage.OwnerId))
				{
					var downloaderEntry =
						await _downloaderAllocator.CreateDownloaderAsync(_options.AgentId, allotDownloaderMessage);

					if (downloaderEntry == null)
					{
						Logger.LogError($"任务 {allotDownloaderMessage.OwnerId} 分配下载器 {allotDownloaderMessage.Type} 失败");
						await _mq.PublishAsync($"{Framework.ResponseHandlerTopic}{allotDownloaderMessage.OwnerId}",
							$"|{Framework.AllocateDownloaderCommand}|false");
					}
					else
					{
						downloaderEntry.LastUsedTime = DateTime.Now;
						ConfigureDownloader?.Invoke(downloaderEntry);

						await _mq.PublishAsync(
							$"{Framework.ResponseHandlerTopic}{allotDownloaderMessage.OwnerId}",
							$"|{Framework.AllocateDownloaderCommand}|true");

						_cache.TryAdd(allotDownloaderMessage.OwnerId, downloaderEntry);

						// 保存分配的下载器初始信息
						File.WriteAllText(
							Path.Combine(Framework.BaseDirectory, DownloaderPersistFolderName,
								allotDownloaderMessage.OwnerId), message, Encoding.UTF8);
						Logger.LogInformation(
							$"任务 {allotDownloaderMessage.OwnerId} 分配下载器 {allotDownloaderMessage.Type} 成功");
					}
				}
				else
				{
					Logger.LogWarning($"任务 {allotDownloaderMessage.OwnerId} 重复分配下载器");
				}
			}
			else
			{
				Logger.LogWarning($"任务 {allotDownloaderMessage.OwnerId} 分配下载器过期");
			}
		}
	}
}