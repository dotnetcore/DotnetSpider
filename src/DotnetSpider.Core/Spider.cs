using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core.Scheduler;
using System.Runtime.CompilerServices;
using System.Reflection;
using Polly;
using Polly.Retry;
using System.IO.MemoryMappedFiles;
using System.Collections.ObjectModel;

[assembly: InternalsVisibleTo("DotnetSpider.Core.Test")]
[assembly: InternalsVisibleTo("DotnetSpider.Sample")]
[assembly: InternalsVisibleTo("DotnetSpider.Extension")]
[assembly: InternalsVisibleTo("DotnetSpider.Extension.Test")]
namespace DotnetSpider.Core
{
	/// <summary>
	/// A spider contains four modules: Downloader, Scheduler, PageProcessor and Pipeline. 
	/// </summary>
	public class Spider : AppBase, ISpider, ISpeedMonitor
	{
		private static readonly object Locker = new object();

		private readonly Site _site;
		private IScheduler _scheduler = new QueueDuplicateRemovedScheduler();
		private IDownloader _downloader = new HttpClientDownloader();
		private Status _realStat = Status.Init;
		private List<ResultItems> _cached;
		private int _waitCountLimit = 1500;
		private bool _init;
		private FileInfo _errorRequestFile;
		private readonly object _avgDownloadTimeLocker = new object();
		private readonly object _avgProcessorTimeLocker = new object();
		private readonly object _avgPipelineTimeLocker = new object();
		private int _threadNum = 1;
		private int _deep = int.MaxValue;
		private bool _skipWhenResultIsEmpty = true;
		private bool _exitWhenComplete = true;
		private int _emptySleepTime = 15000;
		private int _cachedSize = 1;
		private string _identity;
		private StreamWriter _errorRequestStreamWriter;
		private int _errorRequestFlushCount;
		private RetryPolicy _pipelineRetryPolicy;
		private long _requstCount;
		private static readonly object RequestCountLocker = new object();
		private MemoryMappedFile _identityMmf;
		private MemoryMappedFile _taskIdMmf;
		private readonly string[] _closeSignalFiles = new string[2];
		private readonly List<IPipeline> _pipelines = new List<IPipeline>();
		private readonly List<IPageProcessor> _pageProcessors = new List<IPageProcessor>();
		private bool _exited;
		private ulong _downloadCostTime;
		private ulong _processorCostTime;
		private ulong _pipelineCostTime;

		/// <summary>
		/// 是否需要通过StartUrlsBuilder来初始化起始链接
		/// </summary>
		/// <param name="arguments">程序运行参数</param>
		/// <returns>返回 True, 则需要执行所有注册的StartUrlsBulder.</returns>
		protected virtual bool IfRequireBuildStartRequests(string[] arguments)
		{
			return true;
		}

		/// <summary>
		/// 通过StartUrlsBuilder来初始化起始链接后的响应操作
		/// </summary>
		protected virtual void BuildStartRequestsFinished()
		{
		}

		/// <summary>
		/// All pipelines for spider.
		/// </summary>
		protected ReadOnlyCollection<IPipeline> Pipelines => new ReadOnlyCollection<IPipeline>(_pipelines);

		/// <summary>
		/// Storage all processors for spider.
		/// </summary>
		protected ReadOnlyCollection<IPageProcessor> PageProcessors => new ReadOnlyCollection<IPageProcessor>(_pageProcessors);

		/// <summary>
		/// start time of spider.
		/// </summary>
		protected DateTime StartTime { get; private set; }

		/// <summary>
		/// end time of spider.
		/// </summary>
		protected DateTime EndTime { get; private set; } = DateTime.MinValue;

		/// <summary>
		/// Interval time wait for new url.
		/// </summary>
		protected int WaitInterval { get; } = 10;

		/// <summary>
		/// Identity of spider.
		/// </summary>
		public override string Identity
		{
			get => _identity;
			set
			{
				CheckIfRunning();

				if (string.IsNullOrWhiteSpace(value) || value.Length > Env.IdentityMaxLength)
				{
					throw new ArgumentException($"Length of Identity should less than {Env.IdentityMaxLength}.");
				}

				_identity = value;
			}
		}

		/// <summary>
		/// Site of spider.
		/// </summary>
		public Site Site => _site;

		/// <summary>
		/// Whether spider is complete.
		/// </summary>
		public bool IsComplete { get; private set; }

		/// <summary>
		/// Record how many times retried.
		/// </summary>
		public AutomicLong RetriedTimes { get; } = new AutomicLong();

		/// <summary>
		/// Status of spider.
		/// </summary>
		public Status Stat { get; private set; } = Status.Init;

		/// <summary>
		/// Event of crawler a request success.
		/// </summary>
		public event Action<Request> OnSuccess;

		/// <summary>
		/// Event of crawler on comoplete.
		/// </summary>
		public event Action<Spider> OnComplete;

		/// <summary>
		/// Event of crawler on closed.
		/// </summary>
		public event Action<Spider> OnClosed;

		/// <summary>
		/// Whether clear scheduler after spider completed.
		/// </summary>
		public bool ClearSchedulerAfterComplete { get; set; } = true;

		/// <summary>
		/// Monitor of spider.
		/// </summary>
		public IMonitor Monitor { get; set; }

		/// <summary>
		/// Average speed downloader.
		/// </summary>
		public long AvgDownloadSpeed { get; private set; }

		/// <summary>
		/// Average speed processor.
		/// </summary>
		public long AvgProcessorSpeed { get; private set; }

		/// <summary>
		/// Average speed pipeline.
		/// </summary>
		public long AvgPipelineSpeed { get; private set; }

		/// <summary>
		/// 上报运行状态的间隔
		/// </summary>
		public int StatusReportInterval { get; set; } = 5000;

		/// <summary>
		/// Set the retry times for pipeline.
		/// </summary>
		public int PipelineRetryTimes { get; set; } = 1;

		/// <summary>
		/// Scheduler of spider.
		/// </summary>
		public IScheduler Scheduler
		{
			get => _scheduler;
			set
			{
				CheckIfRunning();
				_scheduler = value;
			}
		}

		/// <summary>
		/// The number of request pipeline handled every time.
		/// </summary>
		public int CachedSize
		{
			get => _cachedSize;
			set
			{
				CheckIfRunning();
				_cachedSize = value;
			}
		}

		/// <summary>
		/// Start url builders of spider.
		/// </summary>
		public readonly List<IStartUrlsBuilder> StartUrlBuilders = new List<IStartUrlsBuilder>();

		/// <summary>
		/// Interface used to adsl redial.
		/// </summary>
		public IRedialExecutor RedialExecutor
		{
			get => NetworkCenter.Current.Executor;
			set
			{
				CheckIfRunning();
				NetworkCenter.Current.Executor = value;
			}
		}

		/// <summary>
		/// Downloader of spider.
		/// </summary>
		public IDownloader Downloader
		{
			get => _downloader;
			set
			{
				CheckIfRunning();
				_downloader = value;
			}
		}

		/// <summary>
		/// Spider will exit if there is no any other request after waitting this time.
		/// </summary>
		public int EmptySleepTime
		{
			get => _emptySleepTime;
			set
			{
				CheckIfRunning();

				if (value > 0)
				{
					_emptySleepTime = value;
					_waitCountLimit = value / WaitInterval;
				}
				else if (value == 0)
				{
					_emptySleepTime = 0;
					_waitCountLimit = 0;
				}
				else
				{
					throw new SpiderException("Sleep time should be large than 0.");
				}
			}
		}

		/// <summary>
		/// Whether exit spider after complete.
		/// </summary>
		public bool ExitWhenComplete
		{
			get => _exitWhenComplete;
			set
			{
				CheckIfRunning();
				_exitWhenComplete = value;
			}
		}

		/// <summary>
		/// Thread number of spider.
		/// </summary>
		public int ThreadNum
		{
			get => _threadNum;
			set
			{
				CheckIfRunning();

				if (value <= 0)
				{
					throw new ArgumentException("threadNum should be more than one!");
				}

				_threadNum = value;
			}
		}

		/// <summary>
		/// How deep spider will crawl.
		/// </summary>
		public int Deep
		{
			get => _deep;
			set
			{
				CheckIfRunning();
				_deep = value;
			}
		}

		/// <summary>
		/// Whether skip request when results of processor.
		/// When results of processor is empty will retry request if this value is false.
		/// </summary>
		public bool SkipWhenResultIsEmpty
		{
			get => _skipWhenResultIsEmpty;
			set
			{
				CheckIfRunning();
				_skipWhenResultIsEmpty = value;
			}
		}

		/// <summary>
		/// Monitor to get success count, error count, speed info etc.
		/// </summary>
		public IMonitorable Monitorable => Scheduler;

		/// <summary>
		/// Create a spider with pageProcessors.
		/// </summary>
		/// <param name="site">网站信息</param>
		/// <param name="pageProcessors">页面解析器</param>
		/// <returns>爬虫</returns>
		public static Spider Create(Site site, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, Guid.NewGuid().ToString("N"), new QueueDuplicateRemovedScheduler(),
				pageProcessors, null);
		}

		/// <summary>
		/// Create a spider with pageProcessors and scheduler.
		/// </summary>
		/// <param name="site">网站信息</param>
		/// <param name="pageProcessors">页面解析器</param>
		/// <param name="scheduler">调度队列</param>
		/// <returns>爬虫</returns>
		public static Spider Create(Site site, IScheduler scheduler, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, Guid.NewGuid().ToString("N"), scheduler, pageProcessors, null);
		}

		/// <summary>
		/// Create a spider with pageProcessors and scheduler.
		/// </summary>
		/// <param name="site">网站信息</param>
		/// <param name="identify">唯一标识</param>
		/// <param name="pageProcessors">页面解析器</param>
		/// <param name="scheduler">调度队列</param>
		/// <returns>爬虫</returns>
		public static Spider Create(Site site, string identify, IScheduler scheduler, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, identify, scheduler, pageProcessors, null);
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		protected Spider()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
#endif
			var type = GetType();
			var spiderNameAttribute = type.GetCustomAttribute<TaskName>();
			if (spiderNameAttribute != null)
			{
				Name = spiderNameAttribute.Name;
			}
			else
			{
				Name = type.Name;
			}

			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">站点信息</param>
		public Spider(Site site) : this()
		{
			_site = site ?? new Site();
		}

		/// <summary>
		/// Create a spider with site, identity, scheduler and pageProcessors.
		/// </summary>
		/// <param name="site">网站信息</param>
		/// <param name="identity">唯一标识</param>
		/// <param name="scheduler">调度队列</param>
		/// <param name="pageProcessors">页面解析器</param>
		/// <param name="pipelines">数据管道</param>
		public Spider(Site site, string identity, IScheduler scheduler, IEnumerable<IPageProcessor> pageProcessors, IEnumerable<IPipeline> pipelines) : this(site)
		{
			_identity = identity;
			Scheduler = scheduler;
			if (pageProcessors != null)
			{
				AddPageProcessor(pageProcessors.ToArray());
			}
			if (pipelines != null)
			{
				AddPipelines(pipelines.ToArray());
			}
			CheckIfSettingsCorrect();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="site"></param>
		/// <returns></returns>
		public static Page AddToCycleRetry(Request request, Site site)
		{
			Page page = new Page(request)
			{
				ContentType = site.ContentType
			};

			request.CycleTriedTimes++;

			if (request.CycleTriedTimes <= site.CycleRetryTimes)
			{
				request.Priority = 0;
				page.AddTargetRequest(request, false);
				page.Retry = true;
				return page;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Add start url builder to spider.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public Spider AddStartUrlBuilder(IStartUrlsBuilder builder)
		{
			CheckIfRunning();
			StartUrlBuilders.Add(builder);
			return this;
		}

		/// <summary>
		/// Add startUrls to spider. 
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartUrls(IEnumerable<string> urls)
		{
			CheckIfRunning();
			Site.AddStartUrls(urls);
			return this;
		}

		/// <summary>
		/// Add start requests to spider. 
		/// </summary>
		/// <param name="requests">请求信息</param>
		/// <returns>爬虫</returns>
		public Spider AddStartRequests(IEnumerable<Request> requests)
		{
			CheckIfRunning();
			Site.AddStartRequests(requests);
			return this;
		}

		/// <summary>
		/// Add urls to crawl.
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartUrl(params string[] urls)
		{
			CheckIfRunning();
			Site.AddStartUrls(urls);
			return this;
		}

		/// <summary>
		/// Add urls to spider.
		/// </summary>
		/// <param name="url">链接</param>
		/// <param name="extras">Extra properties of request.</param>
		/// <returns></returns>
		public Spider AddStartUrl(string url, IDictionary<string, dynamic> extras)
		{
			Site.AddStartUrl(url, extras);
			return this;
		}

		/// <summary>
		/// Add urls with information to crawl.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Spider</returns>
		public Spider AddStartRequest(Request request)
		{
			return AddStartRequests(request);
		}

		/// <summary>
		/// Add urls with information to crawl.
		/// </summary>
		/// <param name="requests">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartRequests(params Request[] requests)
		{
			CheckIfRunning();
			Site.AddStartRequests(requests);
			return this;
		}

		/// <summary>
		/// Add a pipeline for Spider
		/// </summary>
		/// <param name="pipeline">数据管道</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPipeline(IPipeline pipeline)
		{
			if (pipeline != null)
			{
				CheckIfRunning();
				_pipelines.Add(pipeline);
			}
			return this;
		}

		/// <summary>
		/// Add page processors to spider.
		/// </summary>
		/// <param name="processors">页面解析器</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPageProcessor(params IPageProcessor[] processors)
		{
			if (processors != null && processors.Length > 0)
			{
				CheckIfRunning();
				foreach (var processor in processors)
				{
					if (processor != null)
					{
						_pageProcessors.Add(processor);
					}
				}
			}
			return this;
		}

		/// <summary>
		/// Set pipelines for Spider
		/// </summary>
		/// <param name="pipelines">数据管道</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPipelines(params IPipeline[] pipelines)
		{
			if (pipelines != null && pipelines.Length > 0)
			{
				CheckIfRunning();
				foreach (var pipeline in pipelines)
				{
					if (pipeline != null)
					{
						AddPipeline(pipeline);
					}
				}
			}
			return this;
		}

		/// <summary>
		/// Used for testing.
		/// </summary>
		/// <returns>All pipelines of spider.</returns>
		public IList<IPipeline> GetPipelines()
		{
			return _pipelines.AsReadOnly();
		}

		/// <summary>
		/// Clear the pipelines set
		/// </summary>
		/// <returns>爬虫</returns>
		public Spider ClearPipeline()
		{
			_pipelines.Clear();
			return this;
		}

		/// <summary>
		/// Run spider.
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void RunApp(params string[] arguments)
		{
			CheckIfSettingsCorrect();

			if (Stat == Status.Running)
			{
				Logger.Log(Identity, "Crawler is running...", Level.Warn);
				return;
			}

			if (_init)
			{
				return;
			}

			InitComponent(arguments);

			if (arguments.Contains("run-test"))
			{
				return;
			}

			if (StartTime == DateTime.MinValue)
			{
				StartTime = DateTime.Now;
			}

			Stat = Status.Running;
			_realStat = Status.Running;
			_exited = false;

			// 计算状态监控器每完成多少个Request则上报状态
			int monitorInterval = CalculateMonitorInterval(Scheduler);

			while (Stat == Status.Running || Stat == Status.Paused)
			{
				// 暂停则一直停在此处
				if (Stat == Status.Paused)
				{
					_realStat = Status.Paused;
					Thread.Sleep(50);
					continue;
				}

				Parallel.For(0, ThreadNum, new ParallelOptions
				{
					MaxDegreeOfParallelism = ThreadNum
				}, i =>
				{
					int waitCount = 1;
					// 每个线程使用一个下载器实例, 在使用如WebDriverDownloader时不需要管理WebDriver实例了
					var downloader = Downloader.Clone();
					while (Stat == Status.Running)
					{
						// 从队列中取出一个请求
						Request request = Scheduler.Poll();

						// 如果队列中没有需要处理的请求, 则开始等待, 一直到设定的 EmptySleepTime 结束, 则认为爬虫应该结束了
						if (request == null)
						{
							if (waitCount > _waitCountLimit && ExitWhenComplete)
							{
								Stat = Status.Finished;
								_realStat = Status.Finished;
								_OnComplete();
								OnComplete?.Invoke(this);
								break;
							}

							// wait until new url added
							WaitNewUrl(ref waitCount);
						}
						else
						{
							waitCount = 1;

							try
							{
								Stopwatch sw = new Stopwatch();
								HandleRequest(sw, request, downloader);
								Thread.Sleep(Site.SleepTime);
							}
							catch (Exception e)
							{
								OnError(request);
								Logger.Log(Identity, $"Crawler {request.Url} failed: {e}.", Level.Error, e);
							}
							finally
							{
								if (request.Proxy != null)
								{
									var statusCode = request.StatusCode;
									Site.HttpProxyPool.ReturnProxy(request.Proxy, statusCode ?? HttpStatusCode.Found);
								}
								lock (RequestCountLocker)
								{
									_requstCount++;

									if (_requstCount % monitorInterval == 0)
									{
										try
										{
											ReportStatus();
											CheckExitSignal();
										}
										catch (Exception e)
										{
											Logger.Log(Identity, $"Report status failed: {e}.", Level.Error);
										}
									}
								}
							}
						}
					}

					SafeDestroy(downloader);
				});
			}
			string msg;
			EndTime = DateTime.Now;
			if (_realStat != Status.Finished)
			{
				_realStat = Status.Exited;
				msg = "Crawl terminated";
			}
			else
			{
				msg = "Crawl complete";
			}
			ReportStatus();
			OnClose();

			Logger.Log(Identity, $"{msg}, cost: {(EndTime - StartTime).TotalSeconds} seconds.", Level.Info);
			PrintInfo.PrintLine();

			OnClosed?.Invoke(this);
			_exited = true;
		}

		/// <summary>
		/// Pause spider.
		/// </summary>
		/// <param name="action">暂停完成后执行的回调</param>
		public void Pause(Action action = null)
		{
			bool isRunning = Stat == Status.Running;
			if (!isRunning)
			{
				Logger.Log(Identity, "Crawler is not running.", Level.Warn);
				action?.Invoke();
			}
			else
			{
				Stat = Status.Paused;
				Logger.Log(Identity, "Stop running...", Level.Warn);
				if (action != null)
				{
					while (_realStat != Status.Paused)
					{
						Thread.Sleep(100);
					}
					action();
				}
			}
		}

		/// <summary>
		/// Contiune spider if spider is paused.
		/// </summary>
		public void Contiune()
		{
			if (_realStat == Status.Paused)
			{
				Stat = Status.Running;
				_realStat = Status.Running;
				Logger.Log(Identity, "Continue...", Level.Warn);
			}
			else
			{
				Logger.Log(Identity, "Crawler was not paused, can not continue...", Level.Warn);
			}
		}

		/// <summary>
		/// 发送退出信号
		/// </summary>
		internal void SendExitSignal()
		{
			if (Env.IsWindows)
			{
				var identityMmf = MemoryMappedFile.OpenExisting(Identity, MemoryMappedFileRights.Write);
				if (identityMmf != null)
				{
					using (MemoryMappedViewStream stream = identityMmf.CreateViewStream())
					{
						var writer = new BinaryWriter(stream);
						writer.Write(1);
					}
				}
				try
				{
					var taskIdMmf = MemoryMappedFile.OpenExisting(TaskId, MemoryMappedFileRights.Write);
					if (taskIdMmf != null)
					{
						using (MemoryMappedViewStream stream = taskIdMmf.CreateViewStream())
						{
							var writer = new BinaryWriter(stream);
							writer.Write(1);
						}
					}
				}
				catch
				{
					//ignore
				}
			}
			else
			{
				File.Create(_closeSignalFiles[0]);
			}
		}

		/// <summary>
		/// Exit spider.
		/// </summary>
		/// <param name="action">退出完成后执行的回调</param>
		public void Exit(Action action = null)
		{
			if (Stat == Status.Running || Stat == Status.Paused)
			{
				Stat = Status.Exited;
				Logger.Log(Identity, "Exit...", Level.Warn);
				return;
			}
			Logger.Log(Identity, "Crawler is not running.", Level.Warn);
			if (action != null)
			{
				Task.Factory.StartNew(() =>
				{
					while (!_exited)
					{
						Thread.Sleep(100);
					}
					action();
				});
			}
		}

		/// <summary>
		/// Dispose spider.
		/// </summary>
		public void Dispose()
		{
			CheckIfRunning();

			int i = 0;
			while (!_exited)
			{
				++i;
				Thread.Sleep(500);
				if (i > 10)
				{
					break;
				}
			}

			OnClose();
		}

		/// <summary>
		/// Check if all settings of spider are correct.
		/// </summary>
		protected void CheckIfSettingsCorrect()
		{
			Identity = (string.IsNullOrWhiteSpace(Identity))
				? CryptoUtil.Md5Encrypt(Guid.NewGuid().ToString())
				: Identity;

			if (Identity.Length > Env.IdentityMaxLength)
			{
				throw new ArgumentException($"Length of Identity should less than {Env.IdentityMaxLength}.");
			}

			if (Site == null)
			{
				throw new ArgumentException($"Site should not be null.");
			}

			if (Site.RemoveOutboundLinks && (Site.Domains == null || Site.Domains.Length == 0))
			{
				throw new ArgumentException($"When you want remove outbound links, the domains should not be null or empty.");
			}
		}

		/// <summary>
		/// Init component of spider.
		/// </summary>
		/// <param name="arguments"></param>
		protected virtual void InitComponent(params string[] arguments)
		{
			PrintInfo.Print();

			Logger.Log(Identity, "Build internal component...", Level.Info);

#if !NET_CORE // 开启多线程支持
			ServicePointManager.DefaultConnectionLimit = 1000;
#endif
			_cached = new List<ResultItems>(CachedSize);

			InitSite();

			InitDownloader();

			InitScheduler(arguments);

			if (PageProcessors == null || PageProcessors.Count == 0)
			{
				throw new SpiderException("Count of PageProcessor is zero.");
			}

			InitPipelines(arguments);

			InitCloseSignal();

			InitMonitor();

			Console.CancelKeyPress += ConsoleCancelKeyPress;

			_waitCountLimit = EmptySleepTime / WaitInterval;

			PrepareErrorRequestsLogFile();

			PipelineRetryTimes = PipelineRetryTimes <= 0 ? 1 : PipelineRetryTimes;

			_pipelineRetryPolicy = Policy.Handle<Exception>().Retry(PipelineRetryTimes, (ex, count) =>
			{
				Logger.NLog($"Execute pipeline failed [{count}]: {ex}", Level.Error);
			});

			ExecuteStartUrlBuilders(arguments);

			PushStartRequestToScheduler();

			_init = true;
		}

		/// <summary>
		/// Get the default pipeline when user forget set a pepeline to spider.
		/// </summary>
		/// <returns>数据管道</returns>
		protected virtual IPipeline GetDefaultPipeline()
		{
			return new NullPipeline();
		}

		/// <summary>
		/// Event when spider on close.
		/// </summary>
		protected void OnClose()
		{
			var containsData = _cached != null && _cached.Count > 0;

			foreach (IPipeline pipeline in _pipelines)
			{
				if (containsData)
				{
					pipeline.Process(_cached.ToArray(), this);
				}
				SafeDestroy(pipeline);
			}

			SafeDestroy(Scheduler);
			SafeDestroy(PageProcessors);
			SafeDestroy(Downloader);

			SafeDestroy(Site.HttpProxyPool);
			SafeDestroy(_errorRequestStreamWriter);
			SafeDestroy(_identityMmf);
			SafeDestroy(_taskIdMmf);
		}

		/// <summary>
		/// Event when spider on complete.
		/// </summary>
		protected virtual void _OnComplete()
		{
			IsComplete = Scheduler.LeftRequestsCount == 0;
			if (ClearSchedulerAfterComplete && IsComplete)
			{
				Scheduler.Clear();
			}
		}

		/// <summary>
		/// Record error request.
		/// </summary>
		/// <param name="request"></param>
		protected void OnError(Request request)
		{
			lock (Locker)
			{
				_errorRequestFlushCount++;
				_errorRequestStreamWriter.WriteLine(request);
				if (_errorRequestFlushCount % 50 == 0)
				{
					_errorRequestStreamWriter.Flush();
				}
			}
			Scheduler.IncreaseErrorCount();
		}

		/// <summary>
		/// Event when spider on success.
		/// </summary>
		protected void _OnSuccess(Request request)
		{
			Scheduler.IncreaseSuccessCount();
			OnSuccess?.Invoke(request);
		}

		/// <summary>
		/// Single/atom logical to handle a request by downloader, processors and pipelines.
		/// </summary>
		/// <param name="stopwatch">计时器</param>
		/// <param name="request">请求信息</param>
		/// <param name="downloader">下载器</param>
		protected void HandleRequest(Stopwatch stopwatch, Request request, IDownloader downloader)
		{
			Page page = null;

			try
			{
				stopwatch.Reset();
				stopwatch.Start();

				page = downloader.Download(request, this);

				stopwatch.Stop();
				CalculateDownloadSpeed(stopwatch.ElapsedMilliseconds);

				if (page == null || page.Skip)
				{
					return;
				}

				if (page.Exception == null)
				{
					stopwatch.Reset();
					stopwatch.Start();

					foreach (var processor in PageProcessors)
					{
						processor.Process(page, this);
					}

					stopwatch.Stop();
					CalculateProcessorSpeed(stopwatch.ElapsedMilliseconds);
				}
				else
				{
					OnError(page.Request);
				}
			}
			catch (DownloadException de)
			{
				if (page != null) OnError(page.Request);
				Logger.Log(Identity, $"Should not catch download exception: {request.Url}.", Level.Error, de);
			}
			catch (Exception e)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				if (page != null) OnError(page.Request);
				Logger.Log(Identity, $"Extract {request.Url} failed, please check your pipeline: {e}.", Level.Error, e);
			}

			if (page == null)
			{
				return;
			}
			// 此处是用于需要循环本身的场景, 不能使用本身Request的原因是Request的尝试次数计算问题
			if (page.Retry)
			{
				RetriedTimes.Inc();
				ExtractAndAddRequests(page);
				return;
			}

			bool excutePipeline = false;
			if (!page.SkipTargetUrls)
			{
				if (page.ResultItems.IsEmpty)
				{
					if (SkipWhenResultIsEmpty)
					{
						Logger.Log(Identity, $"Skip {request.Url} because extract 0 result.", Level.Warn);
						_OnSuccess(request);
					}
					// 场景: 此链接就是用来生产新链接的, 因此不会有内容产出
					else if (page.TargetRequests != null && page.TargetRequests.Count > 0)
					{
						ExtractAndAddRequests(page);
					}
					else
					{
						if (Site.CycleRetryTimes > 0)
						{
							page = AddToCycleRetry(request, Site);
							if (page != null && page.Retry)
							{
								RetriedTimes.Inc();
								ExtractAndAddRequests(page);
							}
							Logger.Log(Identity, $"Download {request.Url} success, retry becuase extract 0 result.", Level.Warn);
						}
						else
						{
							Logger.Log(Identity, $"Download {request.Url} success, will not retry because Site.CycleRetryTimes is 0.", Level.Warn);
							_OnSuccess(request);
						}
					}
				}
				else
				{
					excutePipeline = true;
					ExtractAndAddRequests(page);
				}
			}
			else
			{
				excutePipeline = !page.ResultItems.IsEmpty;
			}

			if (!excutePipeline)
			{
				return;
			}

			if (page.Exception == null)
			{
				stopwatch.Reset();
				stopwatch.Start();

				if (CachedSize == 1)
				{
					foreach (IPipeline pipeline in _pipelines)
					{
						try
						{
							_pipelineRetryPolicy.Execute(() =>
							{
								pipeline.Process(new[] { page.ResultItems }, this);
							});
						}
						catch (Exception e)
						{
							Logger.Log(Identity, $"Execute pipeline failed: {e}", Level.Error);
						}
					}

					StringBuilder builder = new StringBuilder($"Crawl: {request.Url} success");
					if (request.CountOfResults.HasValue)
					{
						builder.Append($", results: { request.CountOfResults}");
					}
					if (request.EffectedRows.HasValue)
					{
						builder.Append($", effectedRow: {request.EffectedRows}");
					}
					builder.Append(".");
					Logger.Log(Identity, builder.ToString(), Level.Info);
				}
				else
				{
					lock (Locker)
					{
						_cached.Add(page.ResultItems);

						Logger.Log(Identity, $"Cached {request.Url} results success.", Level.Info);

						if (_cached.Count >= CachedSize)
						{
							var items = _cached.ToArray();
							_cached.Clear();
							foreach (IPipeline pipeline in _pipelines)
							{
								pipeline.Process(items, this);
							}

							StringBuilder builder = new StringBuilder("Crawl:");
							int countOfResults = 0, effectedRows = 0;
							foreach (var item in items)
							{
								builder.Append($" {request.Url},");
								countOfResults += item.Request.CountOfResults.HasValue ? item.Request.CountOfResults.Value : 0;
								effectedRows += item.Request.EffectedRows.HasValue ? item.Request.EffectedRows.Value : 0;
							}
							builder.Append($" success");
							builder.Append($", results: {countOfResults}");
							builder.Append($", effectedRow: {effectedRows}");
							builder.Append(".");
							Logger.Log(Identity, builder.ToString(), Level.Info);
						}
					}
				}

				_OnSuccess(request);

				stopwatch.Stop();
				CalculatePipelineSpeed(stopwatch.ElapsedMilliseconds);
			}
		}

		/// <summary>
		/// Extract and add target urls to scheduler.
		/// </summary>
		/// <param name="page">页面数据</param>
		protected void ExtractAndAddRequests(Page page)
		{
			if (page.Request.NextDepth <= Deep && page.TargetRequests != null &&
				page.TargetRequests.Count > 0)
			{
				foreach (Request request in page.TargetRequests)
				{
					request.Site = request.Site ?? page.Request.Site;
					Scheduler.Push(request);
				}
			}
		}

		/// <summary>
		/// Check whether spider is running.
		/// </summary>
		protected void CheckIfRunning()
		{
			if (Stat == Status.Running)
			{
				throw new SpiderException("Spider is running.");
			}
		}

		/// <summary>
		/// 初始化调度队列
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected virtual void InitScheduler(params string[] arguments)
		{
			Scheduler = Scheduler ?? new QueueDuplicateRemovedScheduler();
			Scheduler.Init(this);
		}

		/// <summary>
		/// 初始化数据管道
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected virtual void InitPipelines(params string[] arguments)
		{
			if (_pipelines == null || _pipelines.Count == 0)
			{
				var defaultPipeline = GetDefaultPipeline();
				if (defaultPipeline != null)
				{
					_pipelines.Add(defaultPipeline);
				}
			}

			foreach (var pipeline in _pipelines)
			{
				pipeline.Init();
			}
		}

		private void ClearStartRequests()
		{
			lock (Locker)
			{
				Site.StartRequests.Clear();
				GC.Collect();
			}
		}

		private void WaitNewUrl(ref int waitCount)
		{
			Thread.Sleep(WaitInterval);
			++waitCount;
		}

		private void SafeDestroy(object obj)
		{
			if (obj is IDisposable disposable)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception e)
				{
					Logger.Log(Identity, e.ToString(), Level.Error);
				}
			}
		}

		private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Exit();
			while (!_exited)
			{
				Thread.Sleep(100);
			}
		}

		private void CalculateDownloadSpeed(long time)
		{
			lock (_avgDownloadTimeLocker)
			{
				AvgDownloadSpeed = (AvgDownloadSpeed + time) / 2;
			}
		}

		private void CalculateProcessorSpeed(long time)
		{
			lock (_avgProcessorTimeLocker)
			{
				AvgProcessorSpeed = (AvgProcessorSpeed + time) / 2;
			}
		}

		private void CalculatePipelineSpeed(long time)
		{
			lock (_avgPipelineTimeLocker)
			{
				AvgPipelineSpeed = (AvgPipelineSpeed + time) / 2;
			}
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			NetworkCenter.Current.Executor?.Dispose();
			Exit();
			while (!_exited)
			{
				Thread.Sleep(100);
			}
		}

		private void ExecuteStartUrlBuilders(params string[] arguments)
		{
			if (StartUrlBuilders != null && StartUrlBuilders.Count > 0 && IfRequireBuildStartRequests(arguments))
			{
				try
				{
					for (int i = 0; i < StartUrlBuilders.Count; ++i)
					{
						var builder = StartUrlBuilders[i];
						Logger.Log(Identity, $"Add start urls to scheduler via builder[{i + 1}].", Level.Info);
						builder.Build(Site);
					}
				}
				finally
				{
					BuildStartRequestsFinished();
				}
			}
		}

		private void PushStartRequestToScheduler()
		{
			if (Site.StartRequests != null && Site.StartRequests.Count > 0)
			{
				Logger.Log(Identity, $"Add start urls to scheduler, count {Site.StartRequests.Count}.", Level.Info);
				//if ((Scheduler is QueueDuplicateRemovedScheduler) || (Scheduler is PriorityScheduler))
				if ((Scheduler is QueueDuplicateRemovedScheduler))
				{
					foreach (var request in Site.StartRequests)
					{
						Scheduler.Push(request);
					}
				}
				else
				{
					Scheduler.Import(new HashSet<Request>(Site.StartRequests));
					ClearStartRequests();
				}
			}
			else
			{
				Logger.Log(Identity, "Add start urls to scheduler, count 0.", Level.Info);
			}
		}

		private void InitCloseSignal()
		{
			if (Env.IsWindows)
			{
				if (!string.IsNullOrWhiteSpace(Identity))
				{
					_identityMmf = MemoryMappedFile.CreateOrOpen(Identity, 1, MemoryMappedFileAccess.ReadWrite);
					using (MemoryMappedViewStream stream = _identityMmf.CreateViewStream())
					{
						var writer = new BinaryWriter(stream);
						writer.Write(false);
					}
				}
				if (!string.IsNullOrWhiteSpace(TaskId))
				{
					_taskIdMmf = MemoryMappedFile.CreateOrOpen(TaskId, 1, MemoryMappedFileAccess.ReadWrite);
					using (MemoryMappedViewStream stream = _taskIdMmf.CreateViewStream())
					{
						var writer = new BinaryWriter(stream);
						writer.Write(false);
					}
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(Identity))
				{
					_closeSignalFiles[0] = Path.Combine(Env.BaseDirectory, $"{Identity}_cl");
				}
				if (!string.IsNullOrWhiteSpace(TaskId))
				{
					_closeSignalFiles[1] = Path.Combine(Env.BaseDirectory, $"{TaskId}_cl");
				}
				foreach (var closeSignal in _closeSignalFiles)
				{
					if (File.Exists(closeSignal))
					{
						File.Delete(closeSignal);
					}
				}
			}
		}

		private void ReportStatus() => Monitor?.Report(Identity, TaskId, _realStat.ToString(),
					Monitorable.LeftRequestsCount,
					Monitorable.TotalRequestsCount,
					Monitorable.SuccessRequestsCount,
					Monitorable.ErrorRequestsCount,
					AvgDownloadSpeed,
					AvgProcessorSpeed,
					AvgPipelineSpeed,
					ThreadNum);

		private void InitMonitor()
		{
			if (Monitor == null)
			{
				Monitor = string.IsNullOrWhiteSpace(Env.EnterpiseServiceUrl) ? new NLogMonitor() : new HttpMonitor(Env.EnterpiseServiceStatusUrl);
			}
		}

		private int CalculateMonitorInterval(IScheduler scheduler)
		{
			if (scheduler.LeftRequestsCount > 10)
			{
				return 10;
			}

			if (scheduler.LeftRequestsCount > 5)
			{
				return 2;
			}

			return 1;
		}

		private void PrepareErrorRequestsLogFile()
		{
			_errorRequestFile = FileUtil.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));

			while (true)
			{
				try
				{
					if (_errorRequestFile.Exists)
					{
						_errorRequestStreamWriter = new StreamWriter(File.OpenWrite(_errorRequestFile.FullName), Encoding.UTF8);
						break;
					}
					else
					{
						_errorRequestStreamWriter = File.CreateText(_errorRequestFile.FullName);
						break;
					}
				}
				catch
				{
					_errorRequestFile = FileUtil.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, $"errors.{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt"));
				}
				Thread.Sleep(500);
			}
		}

		private void InitSite()
		{
			if (Site.Headers == null)
			{
				Site.Headers = new Dictionary<string, string>();
			}
			Site.Accept = Site.Accept ?? "application/json, text/javascript, */*; q=0.01";
			Site.UserAgent = Site.UserAgent ??
							 "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
			if (!Site.Headers.ContainsKey("Accept-Language"))
			{
				Site.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
			}
		}

		private void InitDownloader()
		{
			Downloader = Downloader ?? new HttpClientDownloader();
		}

		private void CheckExitSignal()
		{
			// MMF 暂时还不支持非WINDOWS操作系统
			if (Env.IsWindows)
			{
				using (MemoryMappedViewStream stream = _identityMmf.CreateViewStream())
				{
					var reader = new BinaryReader(stream);
					if (reader.ReadBoolean())
					{
						Exit();
						return;
					}
				}
				if (_taskIdMmf != null)
				{
					using (MemoryMappedViewStream stream = _taskIdMmf.CreateViewStream())
					{
						var reader = new BinaryReader(stream);
						if (reader.ReadBoolean())
						{
							Exit();
						}
					}
				}
			}
			else
			{
				if (File.Exists(_closeSignalFiles[0]) || File.Exists(_closeSignalFiles[1]))
				{
					Exit();
				}
			}
		}
	}
}