using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.RequestSupplier;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("DotnetSpider.Tests")]
[assembly: InternalsVisibleTo("DotnetSpider.Sample")]

namespace DotnetSpider
{
	/// <summary>
	/// Depth 是独立的系统，只有真的是解析出来的新请求才会导致 Depth 加 1， Depth 一般不能作为 Request 的 Hash 计算，因为不同深度会有相同的链接
	/// 下载和解析导致的重试都不需要更改 Depth, 直接调用下载分发服务，跳过 Scheduler
	/// </summary>
	public partial class Spider
	{
		/// <summary>
		/// 配置选项
		/// </summary>
		protected SpiderOptions Options { get; }
		
		/// <summary>
		/// 日志接口
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// 结束前的处理工作
		/// </summary>
		/// <returns></returns>
		protected virtual Task OnExiting()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="eventBus"></param>
		/// <param name="options"></param>
		/// <param name="logger"></param>
		/// <param name="services">服务提供接口</param>
		/// <param name="statisticsService"></param>
		public Spider(
			IEventBus eventBus,
			IStatisticsService statisticsService,
			SpiderOptions options,
			ILogger<Spider> logger,
			IServiceProvider services)
		{
			_services = services;
			_statisticsService = statisticsService;
			_eventBus = eventBus;
			Options = options;
			Logger = logger;
			Console.CancelKeyPress += ConsoleCancelKeyPress;
		}

		/// <summary>
		/// 设置 Id 为 Guid
		/// </summary>
		public Spider NewGuidId()
		{
			CheckIfRunning();
			Id = Guid.NewGuid().ToString("N");
			return this;
		}

		/// <summary>
		/// 添加请求的配置方法
		/// 可以计算 Cookie, Sign 等操作
		/// </summary>
		/// <param name="configureDelegate">配置方法</param>
		/// <returns></returns>
		public Spider AddConfigureRequestDelegate(Action<Request> configureDelegate)
		{
			Check.NotNull(configureDelegate, nameof(configureDelegate));
			_configureRequestDelegates.Add(configureDelegate);
			return this;
		}

		/// <summary>
		/// 添加数据流处理器
		/// </summary>
		/// <param name="dataFlow">数据流处理器</param>
		/// <returns></returns>
		public Spider AddDataFlow(IDataFlow dataFlow)
		{
			Check.NotNull(dataFlow, nameof(dataFlow));
			CheckIfRunning();
			dataFlow.Logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger(dataFlow.GetType());
			_dataFlows.Add(dataFlow);
			return this;
		}

		/// <summary>
		/// 添加请求供应器
		/// </summary>
		/// <param name="supply">请求供应器</param>
		/// <returns></returns>
		public Spider AddRequestSupply(IRequestSupplier supply)
		{
			Check.NotNull(supply, nameof(supply));
			CheckIfRunning();
			_requestSupplies.Add(supply);
			return this;
		}

		/// <summary>
		/// 添加请求
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public Spider AddRequests(params Request[] requests)
		{
			Check.NotNull(requests, nameof(requests));

			foreach (var request in requests)
			{
				if (!string.IsNullOrWhiteSpace(request.Cookie) && string.IsNullOrWhiteSpace(request.Domain))
				{
					throw new SpiderException("When cookie is not null, domain should not be null");
				}

				request.OwnerId = Id;
				request.Depth = request.Depth == 0 ? 1 : request.Depth;
				_requests.Add(request);
				if (_requests.Count % EnqueueBatchCount == 0)
				{
					EnqueueRequestToScheduler();
				}
			}

			return this;
		}

		/// <summary>
		/// 添加链接
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public Spider AddRequests(params string[] urls)
		{
			Check.NotNull(urls, nameof(urls));

			foreach (var url in urls)
			{
				var request = new Request {Url = url, OwnerId = Id, Depth = 1};
				_requests.Add(request);
				if (_requests.Count % EnqueueBatchCount == 0)
				{
					EnqueueRequestToScheduler();
				}
			}

			return this;
		}

		/// <summary>
		/// 启动爬虫
		/// </summary>
		/// <param name="args">启动参数</param>
		/// <returns></returns>
		public async Task RunAsync(params string[] args)
		{
			CheckIfRunning();

			try
			{
				ResetMmfSignal();

				Logger.LogInformation("初始化爬虫");

				// 定制化的设置
				Initialize();

				var dataFlowInfo = string.Join(" ==> ", _dataFlows.Select(x => x.Name));
				Logger.LogInformation($"DataFlow: {dataFlowInfo}");

				// 设置默认调度器
				_scheduler = _scheduler ?? new QueueDistinctBfsScheduler();

				// 设置状态为: 运行
				Status = Status.Running;

				// 添加任务启动的监控信息
				await _statisticsService.StartAsync(Id);

				// 订阅数据流
				_eventBus.Subscribe($"{Options.ResponseHandlerTopic}{Id}",
					async message => await HandleMessageAsync(message));

				// 订阅命令
				_eventBus.Subscribe($"{Id}",
					async message => await HandleCommandAsync(message));

				Logger.LogInformation($"任务 {Id} 订阅消息队列成功");

				// 初始化各数据流处理器
				foreach (var dataFlow in _dataFlows)
				{
					await dataFlow.InitAsync();
				}

				Logger.LogInformation($"任务 {Id} 数据流处理器初始化完成");

				// 通过供应接口添加请求
				foreach (var requestSupplier in _requestSupplies)
				{
					requestSupplier.Execute(request => AddRequests(request));
				}

				// 把列表中可能剩余的请求加入队列
				EnqueueRequestToScheduler();
				Logger.LogInformation($"任务 {Id} 加载下载请求成功");

				_enqueued.Set(0);
				_responded.Set(0);
				_enqueuedRequestDict.Clear();

				// 启动速度控制器
				StartSpeedControllerAsync().ConfigureAwait(false).GetAwaiter();

				_lastRequestedTime = DateTime.Now;

				// 等待退出信号
				await WaitForExiting();
			}
			catch (Exception e)
			{
				Logger.LogError(e.ToString());
			}
			finally
			{
				foreach (var dataFlow in _dataFlows)
				{
					try
					{
						dataFlow.Dispose();
					}
					catch (Exception ex)
					{
						Logger.LogError($"任务 {Id} 释放 {dataFlow.GetType().Name} 失败: {ex}");
					}
				}

				try
				{
					// TODO: 如果订阅消息队列失败，此处是否应该再尝试上报，会导致两倍的重试时间
					// 添加任务退出的监控信息
					await _statisticsService.ExitAsync(Id);

					// 最后打印一次任务状态信息
					await _statisticsService.PrintStatisticsAsync(Id);
				}
				catch (Exception e)
				{
					Logger.LogInformation($"任务 {Id} 上传退出信息失败: {e}");
				}

				try
				{
					await OnExiting();
				}
				catch (Exception e)
				{
					Logger.LogInformation($"任务 {Id} 退出事件处理失败: {e}");
				}

				// 标识任务退出完成
				Status = Status.Exited;
				Logger.LogInformation($"任务 {Id} 退出");
			}
		}

		/// <summary>
		/// 暂停爬虫
		/// </summary>
		public void Pause()
		{
			Status = Status.Paused;
		}

		/// <summary>
		/// 继续爬虫
		/// </summary>
		public void Continue()
		{
			Status = Status.Running;
		}

		/// <summary>
		/// 退出爬虫
		/// </summary>
		public Spider Exit()
		{
			Logger.LogInformation($"任务 {Id} 退出中...");
			Status = Status.Exiting;
			// 直接取消订阅即可: 1. 如果是本地应用, 
			_eventBus.Unsubscribe($"{Options.ResponseHandlerTopic}{Id}");
			return this;
		}

		/// <summary>
		/// 发送退出信号
		/// </summary>
		public Spider ExitBySignal()
		{
			if (MmfSignal)
			{
				var mmf = MemoryMappedFile.CreateFromFile(Path.Combine("mmf-signal", Id), FileMode.OpenOrCreate, null,
					4,
					MemoryMappedFileAccess.ReadWrite);
				using (var accessor = mmf.CreateViewAccessor())
				{
					accessor.Write(0, true);
					accessor.Flush();
				}

				Logger.LogInformation($"任务 {Id} 推送退出信号到 MMF 成功");
				return this;
			}

			throw new SpiderException($"任务 {Id} 未开启 MMF 控制");
		}

		/// <summary>
		/// 等待任务结束
		/// </summary>
		public void WaitForExit(long milliseconds = 0)
		{
			milliseconds = milliseconds <= 0 ? long.MaxValue : milliseconds;
			var waited = 0;
			while (Status != Status.Exited && waited < milliseconds)
			{
				Thread.Sleep(100);
				waited += 100;
			}
		}

		/// <summary>
		/// 初始化配置
		/// </summary>
		protected virtual void Initialize()
		{
		}

		/// <summary>
		/// 从配置文件中获取数据存储器
		/// </summary>
		/// <returns></returns>
		/// <exception cref="SpiderException"></exception>
		public StorageBase GetDefaultStorage()
		{
			return GetDefaultStorage(Options);
		}

		internal static StorageBase GetDefaultStorage(SpiderOptions options)
		{
			var type = Type.GetType(options.Storage);
			if (type == null)
			{
				throw new SpiderException("存储器类型配置不正确，或者未添加对应的库");
			}

			if (!typeof(StorageBase).IsAssignableFrom(type))
			{
				throw new SpiderException("存储器类型配置不正确");
			}

			var method = type.GetMethod("CreateFromOptions");

			if (method == null)
			{
				throw new SpiderException("存储器未实现 CreateFromOptions 方法，无法自动创建");
			}

			var storage = method.Invoke(null, new object[] {options});
			if (storage == null)
			{
				throw new SpiderException("创建默认存储器失败");
			}

			return (StorageBase) storage;
		}

		private void ResetMmfSignal()
		{
			if (MmfSignal)
			{
				if (!Directory.Exists("mmf-signal"))
				{
					Directory.CreateDirectory("mmf-signal");
				}

				var mmf = MemoryMappedFile.CreateFromFile(Path.Combine("mmf-signal", Id), FileMode.OpenOrCreate, null,
					4, MemoryMappedFileAccess.ReadWrite);
				using (var accessor = mmf.CreateViewAccessor())
				{
					accessor.Write(0, false);
					accessor.Flush();
					Logger.LogInformation("任务 {Id} 初始化 MMF 退出信号");
				}
			}
		}

		/// <summary>
		/// 爬虫速度控制器
		/// </summary>
		/// <returns></returns>
		private Task StartSpeedControllerAsync()
		{
			return Task.Factory.StartNew(async () =>
			{
				bool @break = false;

				var mmf = MmfSignal
					? MemoryMappedFile.CreateFromFile(Path.Combine("mmf-signal", Id), FileMode.OpenOrCreate, null, 4,
						MemoryMappedFileAccess.ReadWrite)
					: null;

				using (var accessor = mmf?.CreateViewAccessor())
				{
					Logger.LogInformation($"任务 {Id} 速度控制器启动");

					var paused = 0;
					while (!@break)
					{
						Thread.Sleep(_speedControllerInterval);

						try
						{
							switch (Status)
							{
								case Status.Running:
								{
									try
									{
										// 判断是否过多下载请求未得到回应
										if (_enqueued.Value - _responded.Value > NonRespondedLimitation)
										{
											if (paused > NonRespondedTimeLimitation)
											{
												Logger.LogInformation(
													$"任务 {Id} {NonRespondedTimeLimitation} 秒未收到下载回应");
												@break = true;
												break;
											}

											paused += _speedControllerInterval;
											Logger.LogInformation($"任务 {Id} 速度控制器因过多下载请求未得到回应暂停");
											continue;
										}

										paused = 0;

										// 重试超时的下载请求
										var timeoutRequests = new List<Request>();
										var now = DateTime.Now;
										foreach (var kv in _enqueuedRequestDict)
										{
											if (!((now - kv.Value.CreationTime).TotalSeconds > RespondedTimeout))
											{
												continue;
											}

											kv.Value.RetriedTimes++;
											if (kv.Value.RetriedTimes > RespondedTimeoutRetryTimes)
											{
												Logger.LogInformation(
													$"任务 {Id} 重试下载请求 {RespondedTimeoutRetryTimes} 次未收到下载回应");
												@break = true;
												break;
											}

											timeoutRequests.Add(kv.Value);
										}

										// 如果有超时的下载则重试，无超时的下载则从调度队列里取
										if (timeoutRequests.Count > 0)
										{
											await EnqueueRequests(timeoutRequests.ToArray());
										}
										else
										{
											var requests = _scheduler.Dequeue(Id, _dequeueBatchCount);

											if (requests == null || requests.Length == 0) break;

											foreach (var request in requests)
											{
												foreach (var configureRequestDelegate in _configureRequestDelegates)
												{
													configureRequestDelegate(request);
												}
											}

											await EnqueueRequests(requests);
										}
									}
									catch (Exception e)
									{
										Logger.LogError($"任务 {Id} 速度控制器运转失败: {e}");
									}

									break;
								}

								case Status.Paused:
								{
									Logger.LogDebug($"任务 {Id} 速度控制器暂停");
									break;
								}

								case Status.Exiting:
								case Status.Exited:
								{
									@break = true;
									break;
								}
							}

							if (!@break && accessor != null && accessor.ReadBoolean(0))
							{
								Logger.LogInformation($"任务 {Id} 收到 MMF 退出信号");
								Exit();
							}
						}
						catch (Exception e)
						{
							Logger.LogError($"任务 {Id} 速度控制器运转失败: {e}");
						}
					}
				}

				Logger.LogInformation($"任务 {Id} 速度控制器退出");
			});
		}

		private Task HandleCommandAsync(Event message)
		{
			switch (message.Type)
			{
				case Framework.ExitCommand:
				{
					Exit();
					break;
				}
			}

			return Task.CompletedTask;
		}

		private async Task HandleMessageAsync(Event message)
		{
			if (string.IsNullOrWhiteSpace(message.Data))
			{
				Logger.LogWarning($"任务 {Id} 接收到空消息");
				return;
			}

			_lastRequestedTime = DateTime.Now;

			Response[] responses;

			try
			{
				responses = JsonConvert.DeserializeObject<Response[]>(message.Data);
			}
			catch
			{
				Logger.LogError($"任务 {Id} 接收到异常消息: {message}");
				return;
			}

			try
			{
				if (responses.Length == 0)
				{
					Logger.LogWarning($"任务 {Id} 接收到空回复");
					return;
				}

				_responded.Add(responses.Length);

				// 只要有回应就从缓存中删除，即便是异常要重新下载会成 EnqueueRequest 中重新加回缓存
				// 此处只需要保证: 发 -> 收 可以一对一删除就可以保证检测机制的正确性
				foreach (var response in responses)
				{
					_enqueuedRequestDict.TryRemove(response.Request.Hash, out _);
				}

				var agentId = responses.First().AgentId;

				var successResponses = responses.Where(x => x.Success).ToList();
				// 统计下载成功
				if (successResponses.Count > 0)
				{
					var elapsedMilliseconds = successResponses.Sum(x => x.ElapsedMilliseconds);
					await _statisticsService.IncrementDownloadSuccessAsync(agentId, successResponses.Count,
						elapsedMilliseconds);
				}

				// 处理下载成功的请求
				Parallel.ForEach(successResponses, async response =>
				{
					Logger.LogInformation($"任务 {Id} 下载 {response.Request.Url} 成功");

					try
					{
						var context = new DataFlowContext(response, _services.CreateScope().ServiceProvider);

						foreach (var dataFlow in _dataFlows)
						{
							var dataFlowResult = await dataFlow.HandleAsync(context);
							var @break = false;
							switch (dataFlowResult)
							{
								case DataFlowResult.Success:
								{
									continue;
								}

								case DataFlowResult.Failed:
								{
									// 如果处理失败，则直接返回
									Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 失败: {context.Result}");
									await _statisticsService.IncrementFailedAsync(Id);
									return;
								}

								case DataFlowResult.Terminated:
								{
									@break = true;
									break;
								}
							}

							if (@break)
							{
								break;
							}
						}

						var resultIsEmpty = !context.HasItems && !context.HasParseItems;
						// 如果解析结果为空，重试
						if (resultIsEmpty && RetryWhenResultIsEmpty)
						{
							if (response.Request.RetriedTimes < response.Request.RetryTimes)
							{
								response.Request.RetriedTimes++;
								await EnqueueRequests(response.Request);
								// 即然是重试这个请求，则解析必然还会再执行一遍，所以解析到的目标链接、成功状态都应该到最后来处理。
								Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 解析结果为空，尝试重试");
								return;
							}
						}

						// 解析的目标请求
						if (context.FollowRequests != null && context.FollowRequests.Count > 0)
						{
							var requests = new List<Request>();
							foreach (var followRequest in context.FollowRequests)
							{
								followRequest.Depth = response.Request.Depth + 1;
								if (followRequest.Depth <= Depth)
								{
									requests.Add(followRequest);
								}
							}

							var count = _scheduler.Enqueue(requests);
							if (count > 0)
							{
								await _statisticsService.IncrementTotalAsync(Id, count);
							}
						}

						if (!resultIsEmpty)
						{
							await _statisticsService.IncrementSuccessAsync(Id);
							Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 成功");
						}
						else
						{
							if (RetryWhenResultIsEmpty)
							{
								await _statisticsService.IncrementFailedAsync(Id);
								Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 失败，解析结果为空");
							}
							else
							{
								await _statisticsService.IncrementSuccessAsync(Id);
								Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 成功，解析结果为空");
							}
						}
					}
					catch (Exception e)
					{
						await _statisticsService.IncrementFailedAsync(Id);
						Logger.LogInformation($"任务 {Id} 处理 {response.Request.Url} 失败: {e}");
					}
				});

				// TODO: 此处需要优化
				var retryResponses =
					responses.Where(x => !x.Success && x.Request.RetriedTimes < x.Request.RetryTimes)
						.ToList();
				var downloadFailedResponses =
					responses.Where(x => !x.Success)
						.ToList();
				var failedResponses =
					responses.Where(x => !x.Success && x.Request.RetriedTimes >= x.Request.RetryTimes)
						.ToList();

				if (retryResponses.Count > 0)
				{
					retryResponses.ForEach(x =>
					{
						x.Request.RetriedTimes++;
						Logger.LogInformation($"任务 {Id} 下载 {x.Request.Url} 失败: {x.Exception}");
					});
					await EnqueueRequests(retryResponses.Select(x => x.Request).ToArray());
				}

				// 统计下载失败
				if (downloadFailedResponses.Count > 0)
				{
					var elapsedMilliseconds = downloadFailedResponses.Sum(x => x.ElapsedMilliseconds);
					await _statisticsService.IncrementDownloadFailedAsync(agentId,
						downloadFailedResponses.Count, elapsedMilliseconds);
				}

				// 统计失败
				if (failedResponses.Count > 0)
				{
					await _statisticsService.IncrementFailedAsync(Id, failedResponses.Count);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError($"任务 {Id} 处理消息 {message} 失败: {ex}");
			}
		}

		/// <summary>
		/// 阻塞等待直到爬虫结束
		/// </summary>
		/// <returns></returns>
		private async Task WaitForExiting()
		{
			int waited = 0;
			while (Status != Status.Exiting)
			{
				if ((DateTime.Now - _lastRequestedTime).Seconds > EmptySleepTime)
				{
					break;
				}
				else
				{
					Thread.Sleep(1000);
				}

				waited += 1;
				if (waited > StatisticsInterval)
				{
					waited = 0;
					await _statisticsService.PrintStatisticsAsync(Id);
				}
			}
		}

		/// <summary>
		/// 判断爬虫是否正在运行
		/// </summary>
		private void CheckIfRunning()
		{
			if (Status == Status.Running || Status == Status.Paused)
			{
				throw new SpiderException("任务 {Id} 正在运行");
			}
		}

		private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Exit();
			while (Status != Status.Exited)
			{
				Thread.Sleep(100);
			}
		}

		/// <summary>
		/// 把当前缓存的所有 Request 入队
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void EnqueueRequestToScheduler()
		{
			if (_requests.Count <= 0) return;

			_scheduler = _scheduler ?? new QueueDistinctBfsScheduler();

			var count = _scheduler.Enqueue(_requests);
			_statisticsService.IncrementTotalAsync(Id, count).ConfigureAwait(false).GetAwaiter();
			Logger.LogInformation($"任务 {Id} 推送请求到调度器，数量 {_requests.Count}");
			_requests.Clear();
		}

		private async Task EnqueueRequests(params Request[] requests)
		{
			if (requests.Length > 0)
			{
				foreach (var request in requests)
				{
					string topic;
					request.CreationTime = DateTime.Now;
					// 初始请求通过是否使用 ADSL 分配不同的下载队列
					if (string.IsNullOrWhiteSpace(request.AgentId))
					{
						topic = request.UseAdsl ? Options.AdslDownloadQueueTopic : Options.DownloadQueueTopic;
					}
					else
					{
						switch (request.DownloadPolicy)
						{
							// 非初始请求如果是链式模式则使用旧的下载器
							case DownloadPolicy.Chained:
							{
								topic = request.AgentId;
								break;
							}

							default:
							{
								topic = request.UseAdsl
									? Options.AdslDownloadQueueTopic
									: Options.DownloadQueueTopic;
								break;
							}
						}
					}

					_enqueuedRequestDict.TryAdd(request.Hash, request);
					await _eventBus.PublishAsync(topic,
						new Event {Type = Framework.DownloadCommand, Data = JsonConvert.SerializeObject(requests)});
				}


				_enqueued.Add(requests.Length);
			}
		}
	}
}