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
using DotnetSpider.MessageQueue;
using DotnetSpider.RequestSupplier;
using DotnetSpider.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("DotnetSpider.Tests")]

namespace DotnetSpider
{
	/// <summary>
	/// Depth 是独立的系统，只有真的是解析出来的新请求才会导致 Depth 加 1， Depth 一般不能作为 Request 的 Hash 计算，因为不同深度会有相同的链接
	/// 下载和解析导致的重试都不需要更改 Depth, 直接调用下载分发服务，跳过 Scheduler
	/// </summary>
	public partial class Spider
	{
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
		public Spider(SpiderParameters spiderParameters)
		{
			Framework.RegisterEncoding();

			Services = spiderParameters.ServiceProvider;
			_statisticsService = spiderParameters.StatisticsService;
			_mq = spiderParameters.Mq;
			Options = spiderParameters.SpiderOptions;
			Logger = spiderParameters.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
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
			dataFlow.Logger = Services.GetRequiredService<ILoggerFactory>().CreateLogger(dataFlow.GetType());
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
		public async Task AddRequests(params Request[] requests)
		{
			Check.NotNull(requests, nameof(requests));

			foreach (var request in requests)
			{
				if (!string.IsNullOrWhiteSpace(request.Cookie) && string.IsNullOrWhiteSpace(request.Domain))
				{
					throw new SpiderException("When cookie is not empty, domain should not be empty");
				}

				request.OwnerId = Id;
				request.Depth = request.Depth == 0 ? 1 : request.Depth;
				_requests.Add(request);
				if (_requests.Count % EnqueueBatchCount == 0)
				{
					await AddRequestsToScheduler();
				}
			}
		}

		/// <summary>
		/// 添加链接
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns></returns>
		public async Task AddRequests(params string[] urls)
		{
			Check.NotNull(urls, nameof(urls));

			foreach (var url in urls)
			{
				var request = new Request {Url = url, OwnerId = Id, Depth = 1};
				_requests.Add(request);
				if (_requests.Count % EnqueueBatchCount == 0)
				{
					await AddRequestsToScheduler();
				}
			}
		}

		/// <summary>
		/// 启动爬虫
		/// </summary>
		/// <param name="args">启动参数</param>
		/// <returns></returns>
		public async Task RunAsync(params string[] args)
		{
			CheckIfRunning();

			PrintEnvironment(Services.GetRequiredService<IConfiguration>());

			try
			{
				ResetMmfSignal();

				Logger.LogInformation("Initialize spider");

				// 定制化的设置
				await Initialize();

				var dataFlowInfo = string.Join(" ==> ", _dataFlows.Select(x => x.Name));
				Logger.LogInformation($"DataFlow: {dataFlowInfo}");

				// 设置默认调度器
				_scheduler = _scheduler ?? new QueueDistinctBfsScheduler();

				// 设置状态为: 运行
				Status = Status.Running;

				// 添加任务启动的监控信息
				await _statisticsService.StartAsync(Id);

				// 订阅数据流
				_mq.Subscribe<Response[]>($"{Options.TopicResponseHandler}{Id}",
					async message => await HandleResponseAsync(message));

				// 订阅命令
				_mq.Subscribe<string>($"{Id}",
					async message => await HandleCommandAsync(message));

				// 初始化各数据流处理器
				foreach (var dataFlow in _dataFlows)
				{
					await dataFlow.InitAsync();
				}

				Logger.LogInformation($"{Id} initialize dataFlows completed");

				// 通过供应接口添加请求
				foreach (var requestSupplier in _requestSupplies)
				{
					requestSupplier.Execute(async request => await AddRequests(request));
				}

				// 把列表中可能剩余的请求加入队列
				await AddRequestsToScheduler();

				_enqueued.Set(0);
				_responded.Set(0);
				_enqueuedRequestDict.Clear();

				// 启动速度控制器
				StartSpeedController();

				_lastRequestedTime = DateTimeOffset.Now;

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
						Logger.LogError($"{Id} dispose {dataFlow.GetType().Name} failed: {ex}");
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
					Logger.LogInformation($"{Id} publish exit message failed: {e}");
				}

				try
				{
					await OnExiting();
				}
				catch (Exception e)
				{
					Logger.LogInformation($"{Id} handle exiting event failed: {e}");
				}

				// 标识任务退出完成
				Status = Status.Exited;
				Logger.LogInformation($"{Id} exited");
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
			Logger.LogInformation($"{Id} exiting...");
			Status = Status.Exiting;
			// 直接取消订阅即可: 1. 如果是本地应用,
			_mq.Unsubscribe($"{Options.TopicResponseHandler}{Id}");
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

				Logger.LogInformation($"{Id} send exit mmf signal success");
				return this;
			}

			throw new SpiderException($"{Id} didn't register mmf");
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
		protected virtual Task Initialize()
		{
			return Task.CompletedTask;
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
				throw new SpiderException($"Storage type {options.Storage} not found");
			}

			if (!typeof(StorageBase).IsAssignableFrom(type))
			{
				throw new SpiderException($"{type} is not a storage dataFlow");
			}

			var method = type.GetMethod("CreateFromOptions");

			if (method == null)
			{
				throw new SpiderException($"Storage {type} didn't implement method CreateFromOptions");
			}

			var storage = method.Invoke(null, new object[] {options});
			if (storage == null)
			{
				throw new SpiderException("Create default storage failed");
			}

			return (StorageBase)storage;
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
					Logger.LogInformation("{Id} initialize mmf exit signal");
				}
			}
		}

		/// <summary>
		/// 爬虫速度控制器
		/// </summary>
		/// <returns></returns>
		private void StartSpeedController()
		{
			Task.Factory.StartNew(async () =>
			{
				var @break = false;

				var mmf = MmfSignal
					? MemoryMappedFile.CreateFromFile(Path.Combine("mmf-signal", Id), FileMode.OpenOrCreate, null, 4,
						MemoryMappedFileAccess.ReadWrite)
					: null;

				using (var accessor = mmf?.CreateViewAccessor())
				{
					Logger.LogInformation($"{Id} start speed controller");

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
													$"{Id} didn't receive response after {NonRespondedTimeLimitation} second");
												@break = true;
												break;
											}

											paused += _speedControllerInterval;
											Logger.LogInformation(
												$"{Id} stop speed controller because too much request timeout");
											continue;
										}

										paused = 0;

										// 重试超时的下载请求
										var timeoutRequests = new List<Request>();
										var now = DateTimeOffset.Now;
										foreach (var kv in _enqueuedRequestDict)
										{
											if (!((now - kv.Value.CreationTime).TotalSeconds > RespondedTimeout))
											{
												continue;
											}

											kv.Value.RetriedTimes++;
											if (kv.Value.RetriedTimes > RespondedTimeoutRetryTimes)
											{
												Logger.LogWarning(
													$"{Id} download {kv.Value.Url} more than {RespondedTimeoutRetryTimes} times");
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

											if (requests == null || requests.Length == 0)
											{
												break;
											}

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
										Logger.LogError($"{Id} speed controller failed: {e}");
									}

									break;
								}

								case Status.Paused:
								{
									Logger.LogDebug($"{Id} pause speed controller");
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
								Logger.LogInformation($"{Id} receive mmf exit signal");
								Exit();
							}
						}
						catch (Exception e)
						{
							Logger.LogError($"{Id} speed controller failed: {e}");
						}
					}
				}

				Logger.LogInformation($"{Id} speed controller exit");
			}).ConfigureAwait(true);
		}

		private Task HandleCommandAsync(MessageData<string> command)
		{
			if (command == null)
			{
				Logger.LogWarning($"{Id} receive empty message");
				return Task.CompletedTask;
			}

			switch (command.Type)
			{
				case Framework.ExitCommand:
				{
					Exit();
					break;
				}
			}

			return Task.CompletedTask;
		}

		private async Task HandleResponseAsync(MessageData<Response[]> message)
		{
			if (message?.Data == null || message.Data.Length == 0)
			{
				Logger.LogWarning($"{Id} receive empty message");
				return;
			}

			_lastRequestedTime = DateTimeOffset.Now;

			var responses = message.Data;

			try
			{
				if (responses.Length == 0)
				{
					Logger.LogWarning($"{Id} receive empty message");
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
					Logger.LogInformation($"{Id} download {response.Request.Url} success");

					try
					{
						using (var scope = Services.CreateScope())
						{
							var context = new DataFlowContext(response, scope.ServiceProvider);

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
										Logger.LogInformation(
											$"{Id} handle {response.Request.Url} failed: {context.Message}");
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

							var resultIsEmpty = !context.HasData && !context.HasParseData;
							// 如果解析结果为空，重试
							if (resultIsEmpty && RetryWhenResultIsEmpty)
							{
								if (response.Request.RetriedTimes < response.Request.RetryTimes)
								{
									response.Request.RetriedTimes++;
									await EnqueueRequests(response.Request);
									// 即然是重试这个请求，则解析必然还会再执行一遍，所以解析到的目标链接、成功状态都应该到最后来处理。
									Logger.LogInformation($"{Id} retry {response.Request.Url} because empty result");
									return;
								}
							}

							// 解析的目标请求
							if (context.ExtraRequests != null && context.ExtraRequests.Count > 0)
							{
								var requests = new List<Request>();
								foreach (var newRequest in context.ExtraRequests)
								{
									newRequest.Depth = response.Request.Depth + 1;
									if (newRequest.Depth <= Depth)
									{
										// 在此强制设制 OwnerId, 防止用户忘记导致出错
										if (string.IsNullOrWhiteSpace(newRequest.OwnerId))
										{
											newRequest.OwnerId = context.Response.Request.OwnerId;
											newRequest.AgentId = context.Response.Request.AgentId;
										}

										requests.Add(newRequest);
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
								Logger.LogInformation($"{Id} handle {response.Request.Url} success");
							}
							else
							{
								if (RetryWhenResultIsEmpty)
								{
									await _statisticsService.IncrementFailedAsync(Id);
									Logger.LogInformation(
										$"{Id} handle {response.Request.Url} failed，extract result is empty");
								}
								else
								{
									await _statisticsService.IncrementSuccessAsync(Id);
									Logger.LogInformation(
										$"{Id} handle {response.Request.Url} success，extract result is empty");
								}
							}
						}
					}
					catch (Exception e)
					{
						await _statisticsService.IncrementFailedAsync(Id);
						Logger.LogInformation($"{Id} handle {response.Request.Url} failed: {e}");
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
						Logger.LogInformation($"{Id} download {x.Request.Url} failed: {x.Exception}");
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
				Logger.LogError($"{Id} handle message {message} failed: {ex}");
			}
		}

		/// <summary>
		/// 阻塞等待直到爬虫结束
		/// </summary>
		/// <returns></returns>
		private async Task WaitForExiting()
		{
			var waited = 0;
			while (Status != Status.Exiting)
			{
				if ((DateTimeOffset.Now - _lastRequestedTime).Seconds > EmptySleepTime)
				{
					break;
				}

				Thread.Sleep(1000);

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
				throw new SpiderException("{Id} is running");
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
		private async Task AddRequestsToScheduler()
		{
			if (_requests.Count <= 0)
			{
				return;
			}

			_scheduler = _scheduler ?? new QueueDistinctBfsScheduler();

			var count = _scheduler.Enqueue(_requests);
			await _statisticsService.IncrementTotalAsync(Id, count);
			Logger.LogInformation($"{Id} enqueue requests to scheduler，count {_requests.Count}");
			_requests.Clear();

			Logger.LogInformation($"{Id} load requests success");
		}

		private async Task EnqueueRequests(params Request[] requests)
		{
			if (requests.Length > 0)
			{
				foreach (var request in requests)
				{
					string topic;
					request.CreationTime = DateTimeOffset.Now;
					// 初始请求通过是否使用 ADSL 分配不同的下载队列
					if (string.IsNullOrWhiteSpace(request.AgentId))
					{
						topic = request.UseAdsl ? Options.TopicAdslDownloadQueue : Options.TopicDownloadQueue;
					}
					else
					{
						switch (request.DownloadPolicy)
						{
							// 非初始请求如果是链式模式则使用旧的下载器
							case DownloadPolicy.Chained:
							{
								topic = $"{request.AgentId}-DownloadQueue";
								break;
							}

							default:
							{
								topic = request.UseAdsl
									? Options.TopicAdslDownloadQueue
									: Options.TopicDownloadQueue;
								break;
							}
						}
					}

					_enqueuedRequestDict.TryAdd(request.Hash, request);
					await _mq.PublishAsync(topic,
						new MessageData<Request[]> {Type = Framework.DownloadCommand, Data = new[] {request}});
				}


				_enqueued.Add(requests.Length);
			}
		}

		static readonly string[] Excludes =
		{
			"Apple_PubSub_Socket_Render", "BUNDLED_TOOLS_PATH", "DEBUGGER_PARENT_PROCESS_PID", "DYLD_LIBRARY_PATH",
			"HOME", "LC_CTYPE", "LOGNAME", "MONO_CFG_DIR", "MONO_CONFIG", "MONO_DEBUG", "MONO_GAC_PREFIX",
			"MONO_GC_PARAMS", "MONO_LOCAL_MACHINE_CERTS", "MONO_PATH", "PATH", "PWD", "RESHARPER_HOST_LOG_DIR",
			"RESHARPER_LOG_CONF", "RIDER_MONO_ARGS", "RIDER_ORIGINAL_DYLD_LIBRARY_PATH",
			"RIDER_ORIGINAL_MONO_CFG_DIR", "RIDER_ORIGINAL_MONO_CONFIG", "RIDER_ORIGINAL_MONO_GAC_PREFIX",
			"RIDER_ORIGINAL_MONO_LOCAL_MACHINE_CERTS", "RIDER_ORIGINAL_MONO_PATH",
			"RIDER_ORIGINAL_MONO_TLS_PROVIDER", "SHELL", "SHLVL", "SSH_AUTH_SOCK", "TERM", "TMPDIR", "USER",
			"VERSIONER_PYTHON_PREFER_32_BIT", "VERSIONER_PYTHON_VERSION", "XPC_FLAGS", "XPC_SERVICE_NAME",
			"_NO_DEBUG_HEAP"
		};

		private void PrintEnvironment(IConfiguration configuration)
		{
			foreach (var kv in configuration.GetChildren())
			{
				if (!string.IsNullOrWhiteSpace(kv.Key) && !Excludes.Contains(kv.Key))
				{
					Logger.LogInformation($"Arg   : {kv.Key} = {kv.Value}", 0, ConsoleColor.DarkYellow);
				}
			}


			Logger.LogInformation($"BaseDirectory   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			Logger.LogInformation(
				$"OS    : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}
