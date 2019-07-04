using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
		private readonly IMessageQueue _mq;
		private readonly DownloaderAgentOptions _options;

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

		public bool IsRunning { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">下载器代理选项</param>
		/// <param name="mq">消息队列</param>
		/// <param name="networkCenter">网络中心</param>
		/// <param name="logger">日志接口</param>
		protected DownloaderAgentBase(
			DownloaderAgentOptions options,
			IMessageQueue mq,
			NetworkCenter networkCenter,
			ILogger logger)
		{
			_mq = mq;
			_options = options;
			Framework.NetworkCenter = networkCenter;
			Logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (IsRunning)
			{
				throw new SpiderException($"下载器代理 {_options.AgentId} 正在运行中");
			}

			// 注册节点
			var json = JsonConvert.SerializeObject(new Entity.DownloaderAgent
			{
				Id = _options.AgentId,
				Name = _options.Name,
				ProcessorCount = Environment.ProcessorCount,
				TotalMemory = Framework.TotalMemory,
				CreationTime = DateTime.Now,
				LastModificationTime = DateTime.Now
			});

			await _mq.PublishAsync(Framework.DownloaderAgentRegisterCenterTopic,
				$"|{Framework.RegisterCommand}|{json}");

			// 订阅节点
			SubscribeMessage();

			IsRunning = true;

			// 开始心跳
			HeartbeatAsync().ConfigureAwait(false).GetAwaiter();

			Logger.LogInformation($"下载器代理 {_options.AgentId} 启动完毕");

			ReleaseDownloaderAsync().ConfigureAwait(false).GetAwaiter();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_mq.Unsubscribe(_options.AgentId);
			_mq.Unsubscribe("DownloadQueue");
			_mq.Unsubscribe("AdslDownloadQueue");
			IsRunning = false;
			Logger.LogInformation($"下载器代理 {_options.AgentId} 退出");

			return Task.CompletedTask;
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
					Logger.LogInformation($"订阅节点 {_options.AgentId} 成功");

					_mq.Subscribe("DownloadQueue", HandleMessage);
					Logger.LogInformation($"订阅全局下载队列 {_options.AgentId} 成功");

					if (_options.SupportAdsl)
					{
						_mq.Subscribe("AdslDownloadQueue", HandleMessage);
						Logger.LogInformation($"订阅 Adsl 下载队列 {_options.AgentId} 成功");
					}

					return;
				}
				catch (Exception e)
				{
					Logger.LogError($"订阅 topic 失败: {e.Message}");
					Thread.Sleep(1000);
				}
			}
		}

		private Task HeartbeatAsync()
		{
			return Task.Factory.StartNew(async () =>
			{
				while (IsRunning)
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

						await _mq.PublishAsync(Framework.DownloaderAgentRegisterCenterTopic,
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

		private Task ReleaseDownloaderAsync()
		{
			return Task.Factory.StartNew(() =>
			{
				while (IsRunning)
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

		private void HandleMessage(string message)
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
					case Framework.DownloadCommand:
					{
						Download(commandMessage.Message).ConfigureAwait(false).GetAwaiter();
						break;
					}

					case Framework.ExitCommand:
					{
						if (commandMessage.Message == _options.AgentId)
						{
							StopAsync(default).ConfigureAwait(true).GetAwaiter();
						}
						else
						{
							Logger.LogWarning($"下载器代理 {_options.AgentId} 收到错误的退出消息: {message}");
						}

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

		private async Task Download(string message)
		{
			var requests = JsonConvert.DeserializeObject<Request[]>(message);

			if (requests.Length > 0)
			{
				// 超时 60 秒的不再下载 
				requests = requests.Where(x => (DateTime.Now - x.CreationTime).TotalSeconds < 60).ToArray();

				var downloader = GetDownloader(requests[0]);
				if (downloader == null)
				{
					Logger.LogError($"未能得到 {requests[0].OwnerId} 的下载器");
				}

				foreach (var request in requests)
				{
					Response response;
					if (downloader == null)
					{
						response = new Response
						{
							Request = request,
							Exception = "任务下载器丢失",
							Success = false,
							AgentId = _options.AgentId
						};
					}
					else
					{
						response = await downloader.DownloadAsync(request);
					}

					_mq.Publish($"{Framework.ResponseHandlerTopic}{request.OwnerId}",
						JsonConvert.SerializeObject(new[] {response}));
				}
			}
			else
			{
				Logger.LogWarning("下载请求数: 0");
			}
		}

		/// <summary>
		/// 分配下载器
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private IDownloader GetDownloader(Request request)
		{
			if (!_cache.ContainsKey(request.OwnerId))
			{
				IDownloader downloader = null;
				switch (request.Type)
				{
					case DownloaderType.Empty:
					{
						downloader = new EmptyDownloader
						{
							AgentId = _options.AgentId,
							Logger = Logger
						};
						break;
					}

					case DownloaderType.Test:
					{
						downloader = new TestDownloader
						{
							AgentId = _options.AgentId,
							Logger = Logger
						};
						break;
					}

					case DownloaderType.Exception:
					{
						downloader = new ExceptionDownloader
						{
							AgentId = _options.AgentId,
							Logger = Logger
						};
						break;
					}

					case DownloaderType.WebDriver:
					{
						throw new NotImplementedException();
					}

					case DownloaderType.HttpClient:
					{
						var httpClient = new HttpClientDownloader
						{
							AgentId = _options.AgentId,
							Logger = Logger,
							UseProxy = request.UseProxy,
							AllowAutoRedirect = request.AllowAutoRedirect,
							Timeout = request.Timeout,
							DecodeHtml = request.DecodeHtml,
							UseCookies = request.UseCookies,
							HttpProxyPool = string.IsNullOrWhiteSpace(_options.ProxySupplyUrl)
								? null
								: new HttpProxyPool(new HttpRowTextProxySupplier(_options.ProxySupplyUrl)),
							RetryTime = request.RetryTimes
						};
						// TODO:
						// httpClient.AddCookies(allotDownloaderMessage.Cookies);
						downloader = httpClient;
						break;
					}
				}

				_cache.TryAdd(request.OwnerId, downloader);
			}

			return _cache[request.OwnerId];
		}
	}
}