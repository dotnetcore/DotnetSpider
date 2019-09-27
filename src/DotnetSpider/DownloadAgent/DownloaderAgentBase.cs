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
using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DownloadAgent
{
	/// <summary>
	/// 下载器代理
	/// </summary>
	public abstract class DownloaderAgentBase : BackgroundService, IDownloaderAgent
	{
		private readonly IMq _mq;
		private readonly DownloaderAgentOptions _options;
		private readonly SpiderOptions _spiderOptions;

		private readonly ILogger _logger;
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
			IMq eventBus,
			NetworkCenter networkCenter,
			ILogger logger)
		{
			_spiderOptions = spiderOptions;
			_mq = eventBus;
			_options = options;
			Framework.NetworkCenter = networkCenter;

			Logger = _mq is ThroughMessageQueue ? null : logger;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _mq.PublishAsync(_spiderOptions.TopicDownloaderAgentRegisterCenter,
				new MessageData<object>
				{
					Type = Framework.RegisterCommand,
					Data = new DownloaderAgent
					{
						Id = _options.AgentId,
						Name = _options.Name,
						ProcessorCount = Environment.ProcessorCount,
						TotalMemory = Framework.TotalMemory,
						CreationTime = DateTimeOffset.Now,
						LastModificationTime = DateTimeOffset.Now
					}
				});
			Logger?.LogInformation($"Agent {_options.AgentId} register success");

			// 订阅节点
			Subscribe();

			await SendHeartbeatAsync();

			// 开始心跳
			StartHeartbeat(stoppingToken);

			ReleaseDownloader(stoppingToken);

			Logger?.LogInformation($"Agent {_options.AgentId} started");
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);

			_mq.Unsubscribe(_options.AgentId);
			_mq.Unsubscribe("DownloadQueue");
			_mq.Unsubscribe("AdslDownloadQueue");

			if (cancellationToken != default)
			{
				// 一小时
				var times = 12 * 60;
				for (var i = 0; i < times; ++i)
				{
					Thread.Sleep(5000);
					Logger?.LogInformation($"Agent {_options.AgentId} is exiting, please exit agent after safe time");
				}
			}
		}

		/// <summary>
		/// 订阅消息
		/// </summary>
		private void Subscribe()
		{
			while (true)
			{
				try
				{
					_mq.Subscribe<string>(_options.AgentId, HandleCommand);

					_mq.Subscribe<Request[]>("DownloadQueue", HandleRequest);

					if (_options.SupportAdsl)
					{
						_mq.Subscribe<Request[]>("AdslDownloadQueue", HandleRequest);
					}

					_mq.Subscribe<Request[]>($"{_options.AgentId}-DownloadQueue", HandleRequest);

					return;
				}
				catch (Exception e)
				{
					Logger?.LogError($"Subscribe topic failed: {e}");
					Thread.Sleep(1000);
				}
			}
		}

		private void StartHeartbeat(CancellationToken stoppingToken)
		{
			Task.Factory.StartNew(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					Thread.Sleep(5000);
					try
					{
						await SendHeartbeatAsync();
					}
					catch (Exception e)
					{
						Logger?.LogDebug($"Agent {_options.AgentId} send heartbeat failed: {e}");
					}
				}
			}, stoppingToken).ConfigureAwait(true);
		}

		private async Task SendHeartbeatAsync()
		{
			await _mq.PublishAsync(_spiderOptions.TopicDownloaderAgentRegisterCenter, new MessageData<object>
			{
				Type = Framework.HeartbeatCommand,
				Data = new DownloaderAgentHeartbeat
				{
					AgentId = _options.AgentId,
					AgentName = _options.Name,
					FreeMemory = (int)Framework.GetFreeMemory(),
					DownloaderCount = _cache.Count,
					CreationTime = DateTimeOffset.Now
				}
			});

			Logger?.LogDebug($"Agent {_options.AgentId} send heartbeat success");
		}

		private void ReleaseDownloader(CancellationToken stoppingToken)
		{
			Task.Factory.StartNew(() =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					Thread.Sleep(1000);

					try
					{
						var now = DateTimeOffset.Now;
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
					}
					catch (Exception e)
					{
						Logger?.LogError($"下载器代理 {_options.AgentId} 释放过期下载器失败: {e}");
					}
				}
			}, stoppingToken).ConfigureAwait(true);
		}

		private void HandleCommand(MessageData<string> message)
		{
			if (message?.Data == null || message.Data.Length == 0)
			{
				Logger?.LogWarning($"Agent {_options.AgentId} receive empty command");
				return;
			}
#if DEBUG
			Logger?.LogDebug($"Agent {_options.AgentId} receive command: {message}");
#endif

			try
			{
				if (message.IsTimeout(60))
				{
					return;
				}

				switch (message.Type)
				{
					case Framework.ExitCommand:
					{
						if (message.Data == _options.AgentId)
						{
							StopAsync(default).ConfigureAwait(true);
						}
						else
						{
							Logger?.LogWarning($"Agent {_options.AgentId} receive wrong command: {message}");
						}

						break;
					}

					default:
					{
						Logger?.LogError($"Agent {_options.AgentId} can't handle command: {message}");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger?.LogError($"Agent {_options.AgentId} handle command: {message} failed: {e}");
			}
		}

		private void HandleRequest(MessageData<Request[]> message)
		{
			if (message?.Data == null || message.Data.Length == 0)
			{
				Logger?.LogWarning($"Agent {_options.AgentId} receive empty message");
				return;
			}
#if DEBUG
			Logger?.LogDebug($"Agent {_options.AgentId} receive message: {message}");
#endif

			try
			{
				if (message.IsTimeout(60))
				{
					return;
				}

				switch (message.Type)
				{
					case Framework.DownloadCommand:
					{
						DownloadAsync(message.Data).ConfigureAwait(true);
						break;
					}

					default:
					{
						Logger?.LogError($"Agent {_options.AgentId} can't handle message: {message}");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger?.LogError($"Agent {_options.AgentId} handle message: {message} failed: {e}");
			}
		}

		private async Task DownloadAsync(Request[] requests)
		{
			if (requests.Length > 0)
			{
				// 超时 60 秒的不再下载
				requests = requests.Where(x => (DateTimeOffset.Now - x.CreationTime).TotalSeconds < 60).ToArray();

				var downloader = GetDownloader(requests[0]);
				if (downloader == null)
				{
					Logger?.LogError($"Can't create/get downloader for {requests[0].OwnerId}");
				}

				List<Response> responses = new List<Response>();
				foreach (var request in requests)
				{
					Response response;
					if (downloader == null)
					{
						response = new Response
						{
							Request = request,
							Exception = "Downloader not found",
							Success = false,
							AgentId = _options.AgentId
						};
					}
					else
					{
						response = await downloader.DownloadAsync(request);
					}
					responses.Add(response);
				}

				foreach (var group in responses.GroupBy(x => x.Request.OwnerId))
				{
					await _mq.PublishAsync($"{_spiderOptions.TopicResponseHandler}{group.Key}",
					   new MessageData<Response[]> { Data = group.ToArray() });
				}
			}
			else
			{
				Logger?.LogWarning("None requests");
			}
		}

		/// <summary>
		/// 分配下载器
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private IDownloader GetDownloader(Request request)
		{
			var key = $"{request.OwnerId}-{request.UseProxy}";
			if (!_cache.ContainsKey(key))
			{
				IDownloader downloader = null;
				switch (request.DownloaderType)
				{
					case DownloaderType.Empty:
					{
						downloader = new EmptyDownloader {AgentId = _options.AgentId, Logger = _logger};
						break;
					}

					case DownloaderType.Test:
					{
						downloader = new TestDownloader {AgentId = _options.AgentId, Logger = _logger};
						break;
					}

					case DownloaderType.Exception:
					{
						downloader = new ExceptionDownloader {AgentId = _options.AgentId, Logger = _logger};
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
							UseCookies = request.UseCookies,
							HttpProxyPool = request.UseProxy
								? string.IsNullOrWhiteSpace(_options.ProxySupplyUrl)
									? null
									: new HttpProxyPool(_logger, new HttpRowTextProxySupplier(_options.ProxySupplyUrl))
								: null,
							RetryTime = request.RetryTimes
						};
						if (!string.IsNullOrWhiteSpace(request.Cookie))
						{
							var cookies = request.Cookie.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
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

				_cache.TryAdd(key, downloader);
			}

			return _cache[key];
		}
	}
}
