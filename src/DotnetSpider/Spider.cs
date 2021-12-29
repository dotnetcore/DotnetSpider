using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bert.RateLimiters;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.MessageQueue;
using DotnetSpider.RequestSupplier;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("DotnetSpider.Tests")]

namespace DotnetSpider
{
	public abstract class Spider :
		BackgroundService
	{
		private readonly IList<IDataFlow> _dataFlows;
		private readonly IList<IRequestSupplier> _requestSuppliers;
		private readonly RequestedQueue _requestedQueue;
		private AsyncMessageConsumer<byte[]> _consumer;
		private readonly DependenceServices _services;
		private readonly string _defaultDownloader;
		private readonly IList<DataParser> _dataParsers;

		/// <summary>
		/// 请求 Timeout 事件
		/// </summary>
		protected event Action<Request[]> OnRequestTimeout;

		/// <summary>
		/// 请求错误事件
		/// </summary>
		protected event Action<Request, Response> OnRequestError;

		/// <summary>
		/// 调度器中无新的请求事件
		/// </summary>
		protected event Action OnSchedulerEmpty;

		protected SpiderOptions Options { get; }

		/// <summary>
		/// 爬虫标识
		/// </summary>
		protected SpiderId SpiderId { get; private set; }

		/// <summary>
		/// 日志接口
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// 是否分布式爬虫
		/// </summary>
		protected bool IsDistributed => _services.MessageQueue.IsDistributed;

		protected Spider(IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger
		)
		{
			Logger = logger;
			Options = options.Value;

			if (Options.Speed > 500)
			{
				throw new SpiderException("Speed should not large than 500");
			}

			_services = services;
			_requestedQueue = new RequestedQueue();
			_requestSuppliers = new List<IRequestSupplier>();
			_dataFlows = new List<IDataFlow>();
			_dataParsers = new List<DataParser>();

			_defaultDownloader = _services.HostBuilderContext.Properties.ContainsKey(Const.DefaultDownloader)
				? _services.HostBuilderContext.Properties[Const.DefaultDownloader]?.ToString()
				: Downloaders.HttpClient;
		}

		/// <summary>
		/// 初始化爬虫数据
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected abstract Task InitializeAsync(CancellationToken stoppingToken = default);

		/// <summary>
		/// 获取爬虫标识和名称
		/// </summary>
		/// <returns></returns>
		protected virtual SpiderId GenerateSpiderId()
		{
			var id = Environment.GetEnvironmentVariable("DOTNET_SPIDER_ID");
			id = string.IsNullOrWhiteSpace(id) ? ObjectId.CreateId().ToString() : id;
			var name = Environment.GetEnvironmentVariable("DOTNET_SPIDER_NAME");
			return new SpiderId(id, name);
		}

		protected IDataFlow GetDefaultStorage()
		{
			return StorageUtilities.CreateStorage(Options.StorageType, _services.Configuration);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_consumer?.Close();
			_services.MessageQueue.CloseQueue(SpiderId.Id);

			await base.StopAsync(cancellationToken);

			Dispose();

			Logger.LogInformation($"{SpiderId} stopped");
		}

		/// <summary>
		/// 配置请求(从 Scheduler 中出队的)
		/// </summary>
		/// <param name="request"></param>
		protected virtual void ConfigureRequest(Request request)
		{
		}

		protected virtual Spider AddRequestSupplier(IRequestSupplier requestSupplier)
		{
			requestSupplier.NotNull(nameof(requestSupplier));
			_requestSuppliers.Add(requestSupplier);
			return this;
		}

		protected virtual Spider AddDataFlow(IDataFlow dataFlow)
		{
			dataFlow.NotNull(nameof(dataFlow));
			_dataFlows.Add(dataFlow);
			return this;
		}

		protected async Task<int> AddRequestsAsync(params string[] requests)
		{
			if (requests == null || requests.Length == 0)
			{
				return 0;
			}

			return await AddRequestsAsync(requests.Select(x => new Request(x)));
		}

		protected async Task<int> AddRequestsAsync(params Request[] requests)
		{
			if (requests == null || requests.Length == 0)
			{
				return 0;
			}

			return await AddRequestsAsync(requests.AsEnumerable());
		}

		protected async Task<int> AddRequestsAsync(IEnumerable<Request> requests)
		{
			if (requests == null)
			{
				return 0;
			}

			var list = new List<Request>();

			foreach (var request in requests)
			{
				if (string.IsNullOrWhiteSpace(request.Downloader)
				    && !string.IsNullOrWhiteSpace(_defaultDownloader))
				{
					request.Downloader = _defaultDownloader;
				}

				if (request.Downloader.Contains("PPPoE") &&
				    string.IsNullOrWhiteSpace(request.PPPoERegex))
				{
					throw new ArgumentException(
						$"Request {request.RequestUri}, {request.Hash} set to use PPPoE but PPPoERegex is empty");
				}

				request.RequestedTimes += 1;

				// 1. 请求次数超过限制则跳过，并添加失败记录
				// 2. 默认构造的请求次数为 0， 并且不允许用户更改，因此可以保证数据安全性
				if (request.RequestedTimes > Options.RetriedTimes)
				{
					await _services.StatisticsClient.IncreaseFailureAsync(SpiderId.Id);
					continue;
				}

				// 1. 默认构造的深度为 0， 并且不允许用户更改，因此可以保证数据安全性
				// 2. 当深度超过限制则跳过
				if (Options.Depth > 0 && request.Depth > Options.Depth)
				{
					continue;
				}

				request.Owner = SpiderId.Id;

				list.Add(request);
			}

			var count = await _services.Scheduler.EnqueueAsync(list);
			return count;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			SpiderId = GenerateSpiderId();
			Logger.LogInformation($"Initialize spider {SpiderId}, {SpiderId.Name}");
			await _services.StatisticsClient.StartAsync(SpiderId.Id, SpiderId.Name);
			await _services.Scheduler.InitializeAsync(SpiderId.Id);
			await InitializeAsync(stoppingToken);
			await InitializeDataFlowsAsync();
			await LoadRequestFromSuppliers(stoppingToken);
			await _services.StatisticsClient.IncreaseTotalAsync(SpiderId.Id, await _services.Scheduler.GetTotalAsync());
			await RegisterConsumerAsync(stoppingToken);
			await RunAsync(stoppingToken);
		}

		private async Task RegisterConsumerAsync(CancellationToken stoppingToken)
		{
			var topic = string.Format(Topics.Spider, SpiderId.Id);

			Logger.LogInformation($"{SpiderId} register topic {topic}");
			_consumer = new AsyncMessageConsumer<byte[]>(topic);
			_consumer.Received += async bytes =>
			{
				object message;
				try
				{
					message = await bytes.DeserializeAsync(stoppingToken);
					if (message == null)
					{
						return;
					}
				}
				catch (Exception e)
				{
					Logger.LogError($"Deserialize message failed: {e}");
					return;
				}

				switch (message)
				{
					case Messages.Spider.Exit exit:
						{
							Logger.LogInformation(
								$"{SpiderId} receive exit message {System.Text.Json.JsonSerializer.Serialize(exit)}");
							if (exit.SpiderId == SpiderId.Id)
							{
								await ExitAsync();
							}

							break;
						}
					case Response response:
						{
							// 1. 从请求队列中去除请求
							// 2. 若是 timeout 的请求，无法通过 Dequeue 获取，会通过 _requestedQueue.GetAllTimeoutList() 获取得到
							var request = _requestedQueue.Dequeue(response.RequestHash);

							if (request != null)
							{
								if (response.StatusCode.IsSuccessStatusCode())
								{
									request.Agent = response.Agent;

									if (IsDistributed)
									{
										Logger.LogInformation(
											$"{SpiderId} download {request.RequestUri}, {request.Hash} via {request.Agent} success");
									}

									// 是否下载成功由爬虫来决定，则非 Agent 自身
									await _services.StatisticsClient.IncreaseAgentSuccessAsync(response.Agent,
										response.ElapsedMilliseconds);
									await HandleResponseAsync(request, response, bytes);
								}
								else
								{
									await _services.StatisticsClient.IncreaseAgentFailureAsync(response.Agent,
										response.ElapsedMilliseconds);
									Logger.LogError(
										$"{SpiderId} download {request.RequestUri}, {request.Hash} status code: {response.StatusCode} failed: {response.ReasonPhrase}");

									// 每次调用添加会导致 Requested + 1, 因此失败多次的请求最终会被过滤不再加到调度队列
									await AddRequestsAsync(request);

									OnRequestError?.Invoke(request, response);
								}
							}

							break;
						}
					default:
						Logger.LogError(
							$"{SpiderId} receive error message {System.Text.Json.JsonSerializer.Serialize(message)}");
						break;
				}
			};

			await _services.MessageQueue.ConsumeAsync(_consumer, stoppingToken);
		}

		private async Task HandleResponseAsync(Request request, Response response, byte[] messageBytes)
		{
			DataFlowContext context = null;
			try
			{
				using var scope = _services.ServiceProvider.CreateScope();
				context = new DataFlowContext(scope.ServiceProvider, Options, request, response)
				{
					MessageBytes = messageBytes
				};

				foreach (var dataFlow in _dataFlows)
				{
					await dataFlow.HandleAsync(context);
				}

				var count = await AddRequestsAsync(context.FollowRequests);
				await _services.StatisticsClient.IncreaseTotalAsync(SpiderId.Id, count);

				// 增加一次成功的请求
				await _services.StatisticsClient.IncreaseSuccessAsync(SpiderId.Id);
				await _services.Scheduler.SuccessAsync(request);
			}
			// DataFlow 可以参过抛出 ExitException 来中止爬虫程序
			catch (ExitException ee)
			{
				Logger.LogError($"Exit: {ee}");
				await ExitAsync();
			}
			catch (Exception e)
			{
				await _services.Scheduler.FailAsync(request);
				// if download correct content, parser or storage failed by network or something else
				// retry it until trigger retryTimes limitation
				await AddRequestsAsync(request);
				Logger.LogError($"{SpiderId} handle {System.Text.Json.JsonSerializer.Serialize(request)} failed: {e}");
			}
			finally
			{
				ObjectUtilities.DisposeSafely(context);
			}
		}

		/// <summary>
		/// 若是没有数据解析器，则认为是不需要数据解析器，直接通到存储器，返回 true
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private bool IsValidRequest(Request request)
		{
			if (_dataParsers == null || _dataParsers.Count == 0)
			{
				return true;
			}

			return _dataParsers.Count > 0 && _dataParsers.Any(x => x.IsValidRequest(request));
		}

		private async Task RunAsync(CancellationToken stoppingToken)
		{
			try
			{
				var sleepTimeLimit = Options.EmptySleepTime * 1000;

				var bucket = CreateBucket(Options.Speed);
				var sleepTime = 0;
				var batch = (int)Options.Batch;
				var start = DateTime.Now;
				var end = start;

				PrintStatistics(stoppingToken);

				while (!stoppingToken.IsCancellationRequested)
				{
					if (_requestedQueue.Count > Options.RequestedQueueCount)
					{
						sleepTime += 10;

						if (await WaitForContinueAsync(sleepTime, sleepTimeLimit, (end - start).TotalSeconds,
							$"{SpiderId} too much requests enqueued"))
						{
							continue;
						}
						else
						{
							break;
						}
					}

					if (await HandleTimeoutRequestAsync())
					{
						continue;
					}

					var requests = (await _services.Scheduler.DequeueAsync(batch)).ToArray();

					if (requests.Length > 0)
					{
						sleepTime = 0;

						foreach (var request in requests)
						{
							ConfigureRequest(request);

							// 若是没有一个 Parser 可以处理此请求，则不需要下载
							// https://github.com/dotnetcore/DotnetSpider/issues/182
							if (!IsValidRequest(request))
							{
								continue;
							}

							while (bucket.ShouldThrottle(1, out var waitTimeMillis))
							{
								await Task.Delay(waitTimeMillis, default);
							}

							if (!await PublishRequestMessagesAsync(request))
							{
								Logger.LogError("Exit by publish request message failed");
								break;
							}
						}

						end = DateTime.Now;
					}
					else
					{
						OnSchedulerEmpty?.Invoke();

						sleepTime += 10;

						if (!await WaitForContinueAsync(sleepTime, sleepTimeLimit, (end - start).TotalSeconds))
						{
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"{SpiderId} exited by exception: {e}");
			}
			finally
			{
				await ExitAsync();
			}
		}

		private async Task<bool> HandleTimeoutRequestAsync()
		{
			var timeoutRequests = _requestedQueue.GetAllTimeoutList();
			if (timeoutRequests.Length <= 0)
			{
				return false;
			}

			foreach (var request in timeoutRequests)
			{
				Logger.LogWarning(
					$"{SpiderId} request {request.RequestUri}, {request.Hash} timeout");
			}

			await AddRequestsAsync(timeoutRequests);

			OnRequestTimeout?.Invoke(timeoutRequests);

			return true;
		}

		private async Task<bool> WaitForContinueAsync(int sleepTime, int sleepTimeLimit, double totalSeconds,
			string waitMessage = null)
		{
			if (sleepTime > sleepTimeLimit)
			{
				Logger.LogInformation($"Exit: {(int)totalSeconds} seconds");
				return false;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(waitMessage))
				{
					Logger.LogInformation(waitMessage);
				}

				await Task.Delay(10, default);
				return true;
			}
		}

		private void PrintStatistics(CancellationToken stoppingToken)
		{
			if (!IsDistributed)
			{
				Task.Factory.StartNew(async () =>
				{
					while (!stoppingToken.IsCancellationRequested)
					{
						await Task.Delay(5000, stoppingToken);
						await _services.StatisticsClient.PrintAsync(SpiderId.Id);
					}
				}, stoppingToken).ConfigureAwait(false).GetAwaiter();
			}
		}

		private async Task ExitAsync()
		{
			await _services.StatisticsClient.ExitAsync(SpiderId.Id);
			_services.ApplicationLifetime.StopApplication();
		}

		private static FixedTokenBucket CreateBucket(double speed)
		{
			if (speed >= 1)
			{
				var defaultTimeUnit = (int)(1000 / speed);
				return new FixedTokenBucket(1, 1, defaultTimeUnit);
			}
			else
			{
				var defaultTimeUnit = (int)((1 / speed) * 1000);
				return new FixedTokenBucket(1, 1, defaultTimeUnit);
			}
		}

		private async Task<bool> PublishRequestMessagesAsync(params Request[] requests)
		{
			if (requests.Length > 0)
			{
				foreach (var request in requests)
				{
					string topic;
					request.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
					if (string.IsNullOrWhiteSpace(request.Agent))
					{
						topic = string.IsNullOrEmpty(request.Downloader)
							? Downloaders.HttpClient
							: request.Downloader;
					}
					else
					{
						switch (request.Policy)
						{
							// 非初始请求如果是链式模式则使用旧的下载器
							case RequestPolicy.Chained:
								{
									topic = $"{request.Agent}";
									break;
								}
							case RequestPolicy.Random:
								{
									topic = string.IsNullOrEmpty(request.Downloader)
										? Downloaders.HttpClient
										: request.Downloader;
									break;
								}
							default:
								{
									throw new ApplicationException($"Not supported policy: {request.Policy}");
								}
						}
					}

					if (_requestedQueue.Enqueue(request))
					{
						await _services.MessageQueue.PublishAsBytesAsync(topic, request);
					}
					else
					{
						Logger.LogWarning(
							$"{SpiderId} enqueue request: {request.RequestUri}, {request.Hash} failed");
					}
				}
			}

			return true;
		}

		protected async Task LoadRequestFromSuppliers(CancellationToken stoppingToken)
		{
			// 通过供应接口添加请求
			foreach (var requestSupplier in _requestSuppliers)
			{
				foreach (var request in await requestSupplier.GetAllListAsync(stoppingToken))
				{
					await AddRequestsAsync(request);
				}

				Logger.LogInformation(
					$"{SpiderId} load request from {requestSupplier.GetType().Name} {_requestSuppliers.IndexOf(requestSupplier)}/{_requestSuppliers.Count}");
			}
		}

		private async Task InitializeDataFlowsAsync()
		{
			if (_dataFlows.Count == 0)
			{
				Logger.LogWarning($"{SpiderId} there is no any dataFlow");
			}
			else
			{
				var dataFlowInfo = string.Join(" -> ", _dataFlows.Select(x => x.GetType().Name));
				Logger.LogInformation($"{SpiderId} DataFlows: {dataFlowInfo}");
				foreach (var dataFlow in _dataFlows)
				{
					dataFlow.SetLogger(Logger);
					try
					{
						await dataFlow.InitializeAsync();
						if (dataFlow is DataParser dataParser)
						{
							_dataParsers.Add(dataParser);
						}
					}
					catch (Exception e)
					{
						Logger.LogError(
							$"{SpiderId} initialize dataFlow {dataFlow.GetType().Name} failed: {e}");
						_services.ApplicationLifetime.StopApplication();
					}
				}
			}
		}

		public override void Dispose()
		{
			ObjectUtilities.DisposeSafely(Logger, _requestedQueue);
			ObjectUtilities.DisposeSafely(Logger, _dataFlows);
			ObjectUtilities.DisposeSafely(Logger, _services);

			base.Dispose();

			GC.Collect();
		}
	}
}
