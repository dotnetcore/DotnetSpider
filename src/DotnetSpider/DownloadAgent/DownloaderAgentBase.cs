using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Network;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.DownloadAgent
{
	/// <summary>
	/// 下载器代理
	/// </summary>
	public abstract class DownloaderAgentBase : BackgroundService, IDownloaderAgent
	{
		private readonly IEventBus _eventBus;
		private readonly DownloaderAgentOptions _options;
		private readonly SpiderOptions _spiderOptions;
		private bool _exit;

		private readonly ConcurrentDictionary<string, IDownloader> _cache =
			new ConcurrentDictionary<string, IDownloader>();

		/// <summary>
		/// 日志接口
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="options">下载器代理选项</param>
		/// <param name="spiderOptions"></param>
		/// <param name="eventBus">消息队列</param>
		/// <param name="networkCenter">网络中心</param>
		/// <param name="logger">日志接口</param>
		protected DownloaderAgentBase(
			DownloaderAgentOptions options,
			SpiderOptions spiderOptions,
			IEventBus eventBus,
			NetworkCenter networkCenter,
			ILogger logger)
		{
			_spiderOptions = spiderOptions;
			_eventBus = eventBus;
			_options = options;
			Framework.NetworkCenter = networkCenter;

			Logger = _eventBus is LocalEventBus ? null : logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _eventBus.PublishAsync(_spiderOptions.TopicDownloaderAgentRegisterCenter, new Event
			{
				Type = Framework.RegisterCommand,
				Data = JsonConvert.SerializeObject(new DownloadAgentRegisterCenter.Entity.DownloaderAgent
				{
					Id = _options.AgentId,
					Name = _options.Name,
					ProcessorCount = Environment.ProcessorCount,
					TotalMemory = Framework.TotalMemory,
					CreationTime = DateTime.Now,
					LastModificationTime = DateTime.Now
				})
			});

			// 订阅节点
			SubscribeMessage();

			await SendHeartbeatAsync();

			// 开始心跳
			HeartbeatAsync(stoppingToken).ConfigureAwait(false).GetAwaiter();

			ReleaseDownloaderAsync(stoppingToken).ConfigureAwait(false).GetAwaiter();

			Logger?.LogInformation($"下载器代理 {_options.AgentId} 启动完毕");
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_exit = true;
			_eventBus.Unsubscribe(_options.AgentId);
			_eventBus.Unsubscribe("DownloadQueue");
			_eventBus.Unsubscribe("AdslDownloadQueue");


			// 一小时
			var times = 12 * 60;
			for (int i = 0; i < times && !cancellationToken.IsCancellationRequested; ++i)
			{
				Thread.Sleep(5000);
				Logger?.LogInformation($"下载器代理 {_options.AgentId} 退出中, 请在安全的时间后手动退出节点");
			}


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
					_eventBus.Subscribe(_options.AgentId, HandleMessage);
					Logger?.LogInformation($"订阅节点 {_options.AgentId} 成功");

					_eventBus.Subscribe("DownloadQueue", HandleMessage);
					Logger?.LogInformation($"订阅全局下载队列 {_options.AgentId} 成功");

					if (_options.SupportAdsl)
					{
						_eventBus.Subscribe("AdslDownloadQueue", HandleMessage);
						Logger?.LogInformation($"订阅 Adsl 下载队列 {_options.AgentId} 成功");
					}

					return;
				}
				catch (Exception e)
				{
					Logger?.LogError($"订阅 topic 失败: {e.Message}");
					Thread.Sleep(1000);
				}
			}
		}

		private Task HeartbeatAsync(CancellationToken stoppingToken)
		{
			return Task.Factory.StartNew(async () =>
			{
				while (!stoppingToken.IsCancellationRequested && !_exit)
				{
					Thread.Sleep(5000);
					try
					{
						await SendHeartbeatAsync();
					}
					catch (Exception e)
					{
						Logger?.LogDebug($"下载器代理 {_options.AgentId} 发送心跳失败: {e}");
					}
				}
			}, stoppingToken);
		}

		private async Task SendHeartbeatAsync()
		{
			var json = JsonConvert.SerializeObject(new DownloaderAgentHeartbeat
			{
				AgentId = _options.AgentId,
				AgentName = _options.Name,
				FreeMemory = (int) Framework.GetFreeMemory(),
				DownloaderCount = _cache.Count,
				CreationTime = DateTime.Now
			});

			await _eventBus.PublishAsync(_spiderOptions.TopicDownloaderAgentRegisterCenter,
				new Event
				{
					Type = Framework.HeartbeatCommand,
					Data = json
				});
			Logger?.LogDebug($"下载器代理 {_options.AgentId} 发送心跳成功");
		}

		private Task ReleaseDownloaderAsync(CancellationToken stoppingToken)
		{
			return Task.Factory.StartNew(() =>
			{
				while (!stoppingToken.IsCancellationRequested && !_exit)
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
								Logger?.LogWarning($"下载器代理 {_options.AgentId} 释放过期下载器 {expire} 失败");
							}
						}

						var msg = $"下载器代理 {_options.AgentId} 释放过期下载器: {expires.Count}";
						if (expires.Count > 0)
						{
							Logger?.LogInformation(msg);
						}
						else
						{
							// Logger.LogDebug(msg);
						}
					}
					catch (Exception e)
					{
						Logger?.LogError($"下载器代理 {_options.AgentId} 释放过期下载器失败: {e}");
					}
				}
			}, stoppingToken);
		}

		private void HandleMessage(Event message)
		{
			if (message == null)
			{
				Logger?.LogWarning($"下载器代理 {_options.AgentId} 接收到空消息");
				return;
			}
#if DEBUG
			Logger?.LogDebug($"下载器代理 {_options.AgentId} 接收到消息: {message}");
#endif

			try
			{
				switch (message.Type)
				{
					case Framework.DownloadCommand:
					{
						if (message.IsTimeout(60))
						{
							break;
						}

						DownloadAsync(message.Data).ConfigureAwait(false).GetAwaiter();
						break;
					}

					case Framework.ExitCommand:
					{
						if (message.IsTimeout(6))
						{
							break;
						}

						if (message.Data == _options.AgentId)
						{
							StopAsync(default).ConfigureAwait(true).GetAwaiter();
						}
						else
						{
							Logger?.LogWarning($"下载器代理 {_options.AgentId} 收到错误的退出消息: {message}");
						}

						break;
					}

					default:
					{
						Logger?.LogError($"下载器代理 {_options.AgentId} 无法处理消息: {message}");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger?.LogError($"下载器代理 {_options.AgentId} 处理消息: {message} 失败, 异常: {e}");
			}
		}

		private async Task DownloadAsync(string message)
		{
			var requests = JsonConvert.DeserializeObject<Request[]>(message);

			if (requests.Length > 0)
			{
				// 超时 60 秒的不再下载 
				requests = requests.Where(x => (DateTime.Now - x.CreationTime).TotalSeconds < 60).ToArray();

				var downloader = GetDownloader(requests[0]);
				if (downloader == null)
				{
					Logger?.LogError($"未能得到 {requests[0].OwnerId} 的下载器");
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

					await _eventBus.PublishAsync($"{_spiderOptions.TopicResponseHandler}{request.OwnerId}",
						new Event
						{
							Data = JsonConvert.SerializeObject(new[] {response})
						});
				}
			}
			else
			{
				Logger?.LogWarning("下载请求数: 0");
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
				switch (request.DownloaderType)
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
						if (!string.IsNullOrWhiteSpace(request.Cookie))
						{
							var cookies = request.Cookie.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
							foreach (var cookie in cookies)
							{
								var splitIndex = cookie.IndexOf('=');
								if (splitIndex > 0)
								{
									var name = cookie.Substring(0, splitIndex);
									var value = cookie.Substring(splitIndex + 1, cookie.Length - splitIndex - 1);
									httpClient.AddCookies(new Cookie(name, value, request.Domain));
								}
							}
						}

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