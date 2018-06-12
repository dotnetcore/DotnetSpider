using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Text;

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
		private Site _site = new Site();
		private IScheduler _scheduler = new QueueDuplicateRemovedScheduler();
		private IDownloader _downloader = new HttpClientDownloader();
		protected readonly List<IPipeline> _pipelines = new List<IPipeline>();
		protected readonly List<IPageProcessor> _pageProcessors = new List<IPageProcessor>();
		private List<ResultItems> _cached;
		private int _waitCountLimit = 1500;
		private bool _inited;
		private FileInfo _errorRequestsLogFile;
		private readonly object _avgDownloadTimeLocker = new object();
		private readonly object _avgProcessorTimeLocker = new object();
		private readonly object _avgPipelineTimeLocker = new object();
		private int _threadNum = 1;
		private bool _skipTargetUrlsWhenResultIsEmpty = true;
		private bool _exitWhenComplete = true;
		private int _emptySleepTime = 15000;
		private int _pipelineCachedSize = 1;
		private string _identity = Guid.NewGuid().ToString("N");
		private StreamWriter _errorRequestStreamWriter;
		private int _errorRequestFlushCount;
		private RetryPolicy _pipelineRetryPolicy;
		private AutomicLong _requestedCount = new AutomicLong(0);
		private MemoryMappedFile _identityMmf;
		private MemoryMappedFile _taskIdMmf;
		private readonly string[] _closeSignalFiles = new string[2];
		private bool _exited;
		private IMonitor _monitor;
		private readonly List<IStartUrlsBuilder> _startUrlsBuilders = new List<IStartUrlsBuilder>();
		private int _pipelineRetryTimes = 2;
		private int _statusReportInterval = 5000;
		private int _monitorReportInterval;

		/// <summary>
		/// 是否需要通过StartUrlsBuilder来初始化起始链接
		/// </summary>
		/// <param name="arguments">程序运行参数</param>
		/// <returns>返回 True, 则需要执行所有注册的StartUrlsBulder.</returns>
		protected virtual bool IfRequireBuildStartUrlsBuilders(string[] arguments)
		{
			return arguments.Any(t => t?.ToLower() == "nostartrequestbuild");
		}

		/// <summary>
		/// 通过StartUrlsBuilder来初始化起始链接后的响应操作
		/// </summary>
		protected virtual void BuildStartUrlsBuildersCompleted()
		{
		}

		/// <summary>
		/// All pipelines for spider.
		/// </summary>
		public IReadOnlyCollection<IPipeline> Pipelines => new ReadOnlyEnumerable<IPipeline>(_pipelines);

		/// <summary>
		/// Storage all processors for spider.
		/// </summary>
		public IReadOnlyCollection<IPageProcessor> PageProcessors => new ReadOnlyEnumerable<IPageProcessor>(_pageProcessors);

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

				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(Identity)} should not be empty or null.");
				}
				if (value.Length > Env.IdentityMaxLength)
				{
					throw new ArgumentException($"Length of identity should less than {Env.IdentityMaxLength}.");
				}

				_identity = value;
			}
		}

		/// <summary>
		/// Site of spider.
		/// </summary>
		public Site Site
		{
			get => _site;
			protected set
			{
				_site = value ?? throw new ArgumentException($"{nameof(Site)} should not be null.");
			}
		}

		/// <summary>
		/// Whether spider is complete.
		/// </summary>
		public bool IsCompleted { get; private set; }

		/// <summary>
		/// Record how many times retried.
		/// </summary>
		public AutomicLong RetriedTimes { get; } = new AutomicLong();

		/// <summary>
		/// Status of spider.
		/// </summary>
		public Status Status { get; private set; } = Status.Init;

		/// <summary>
		/// Event of crawler a request success.
		/// </summary>
		public event Action<Request> OnRequestSucceeded;

		/// <summary>
		/// Event of crawler on comoplete.
		/// </summary>
		public event Action<Spider> OnCompleted;

		/// <summary>
		/// Event of crawler on closed.
		/// </summary>
		public event Action<Spider> OnClosed;

		/// <summary>
		/// Whether clear scheduler after spider completed.
		/// 爬虫完成的定义是指队列中再也没有需要采集的请求, 而不是爬虫退出. 对于内存型爬虫来说, 这个值的设置没有关系, 因为程序关闭后内存自然释放掉了. 此值主要是用在分布式队列
		/// 中, 队列中的Request数量为0则表达整个爬虫结束, 如果此值为true, 则需要调用分布式队列的清空方法(分布式队列中会保存所以已经采集过的Request, 以及所有Request用于判断去重, 
		/// 对于量大的爬虫会导到分布式队列的存储爆炸, 所以需要清理)
		/// </summary>
		public bool ClearSchedulerAfterCompleted { get; set; } = true;

		/// <summary>
		/// Monitor of spider.
		/// </summary>
		public IMonitor Monitor
		{
			get => _monitor;
			set
			{
				CheckIfRunning();
				_monitor = value;
				_monitor.Logger = Logger;
			}
		}

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
		public int StatusReportInterval
		{
			get => _statusReportInterval;
			set
			{
				CheckIfRunning();

				if (value < 2000)
				{
					throw new ArgumentException($"{nameof(StatusReportInterval)} should greater than 2000.");
				}
				if (value > 60000)
				{
					throw new ArgumentException($"{nameof(StatusReportInterval)} should less than 60000.");
				}
				_statusReportInterval = value;
			}
		}

		/// <summary>
		/// Set the retry times for pipeline.
		/// </summary>
		public int PipelineRetryTimes
		{
			get => _pipelineRetryTimes;
			set
			{
				CheckIfRunning();

				if (value <= 0)
				{
					throw new ArgumentException($"{nameof(PipelineRetryTimes)} should greater than 0.");
				}

				_pipelineRetryTimes = value;
			}
		}

		/// <summary>
		/// Scheduler of spider.
		/// </summary>
		public IScheduler Scheduler
		{
			get => _scheduler;
			set
			{
				CheckIfRunning();
				_scheduler = value ?? throw new ArgumentNullException($"{nameof(Scheduler)} should not be null.");
			}
		}

		/// <summary>
		/// The number of request pipeline handled every time.
		/// </summary>
		public int PipelineCachedSize
		{
			get => _pipelineCachedSize;
			set
			{
				CheckIfRunning();
				if (value <= 0)
				{
					throw new ArgumentException($"{nameof(PipelineCachedSize)} should be greater than 0.");
				}
				_pipelineCachedSize = value;
			}
		}

		/// <summary>
		/// Start url builders of spider.
		/// </summary>
		public IReadOnlyCollection<IStartUrlsBuilder> StartUrlBuilders => new ReadOnlyEnumerable<IStartUrlsBuilder>(_startUrlsBuilders);

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
				_downloader = value ?? throw new ArgumentNullException($"{nameof(Downloader)} should not be null.");
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
					throw new SpiderException($"{nameof(EmptySleepTime)} should be greater than 0.");
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
					throw new ArgumentException($"{nameof(ThreadNum)} should be more than one!");
				}

				_threadNum = value;
			}
		}

		/// <summary>
		/// Whether skip request when results of processor.
		/// When results of processor is empty will retry request if this value is false.
		/// </summary>
		public bool SkipTargetUrlsWhenResultIsEmpty
		{
			get => _skipTargetUrlsWhenResultIsEmpty;
			set
			{
				CheckIfRunning();
				_skipTargetUrlsWhenResultIsEmpty = value;
			}
		}

		/// <summary>
		/// Monitor to get success count, error count, speed info etc.
		/// </summary>
		public IMonitorable Monitorable => Scheduler;

		/// <summary>
		/// 构造方法
		/// </summary>
		protected Spider()
		{
#if NETSTANDARD
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
#endif
			var type = GetType();
			var taskNameAttribute = type.GetCustomAttribute<TaskName>();
			Name = taskNameAttribute != null ? taskNameAttribute.Name : type.Name;

			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			//var config = new ConfigurationBuilder()
			//	.SetBasePath(Directory.GetCurrentDirectory())
			//	.AddXmlFile("app.config")
			//	.Build();

			//Log.Logger=   new LoggerConfiguration()
			//	.Enrich.FromLogContext()
			//	.ReadFrom.Configuration(Configuration)
			//	.WriteTo.Console().WriteTo.File("DNIC.Erechtheion.log")
			//	.CreateLogger();
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">站点信息</param>
		public Spider(Site site) : this()
		{
			Site = site;
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
			Identity = identity;
			Scheduler = scheduler;
			if (pageProcessors != null)
			{
				AddPageProcessors(pageProcessors);
			}
			if (pipelines != null)
			{
				AddPipelines(pipelines);
			}

			ValidateSettings();
		}

		/// <summary>
		/// Create a spider with pageProcessors.
		/// 不需要指定标识, 使用内存队列, 使用默认HttpClient下载器, 允许没有Pipeline
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
		/// Add start url builder to spider.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public Spider AddStartUrlBuilder(IStartUrlsBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException($"{nameof(builder)} should not be null.");
			}
			CheckIfRunning();
			_startUrlsBuilders.Add(builder);
			return this;
		}

		/// <summary>
		/// Add a url to spider.
		/// </summary>
		/// <param name="url">链接</param>
		/// <param name="extras">Extra properties of request.</param>
		/// <returns></returns>
		public Spider AddStartUrl(string url)
		{
			return AddStartUrls(url);
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
		/// Add urls to crawl.
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartUrls(params string[] urls)
		{
			if (urls == null)
			{
				throw new ArgumentNullException($"{nameof(urls)} should not be null.");
			}
			return AddStartUrls(urls.AsEnumerable());
		}

		/// <summary>
		/// Add startUrls to spider. 
		/// </summary>
		/// <param name="urls">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartUrls(IEnumerable<string> urls)
		{
			if (urls == null)
			{
				throw new ArgumentNullException($"{nameof(urls)} should not be null.");
			}
			CheckIfRunning();
			Site.AddStartUrls(urls);
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
			if (requests == null)
			{
				throw new ArgumentNullException($"{nameof(requests)} should not be null.");
			}
			return AddStartRequests(requests.AsEnumerable());
		}

		/// <summary>
		/// Add urls with information to crawl.
		/// </summary>
		/// <param name="requests">链接</param>
		/// <returns>爬虫</returns>
		public Spider AddStartRequests(IEnumerable<Request> requests)
		{
			if (requests == null)
			{
				throw new ArgumentNullException($"{nameof(requests)} should not be null.");
			}
			CheckIfRunning();
			Site.AddStartRequests(requests);
			return this;
		}

		/// <summary>
		/// Add a page processor to spider.
		/// </summary>
		/// <param name="processors">页面解析器</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPageProcessor(IPageProcessor processor)
		{
			return AddPageProcessors(processor);
		}

		/// <summary>
		/// Add page processors to spider.
		/// </summary>
		/// <param name="processors">页面解析器</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPageProcessors(params IPageProcessor[] processors)
		{
			if (processors == null)
			{
				throw new ArgumentNullException($"{nameof(processors)} should not be null.");
			}
			return AddPageProcessors(processors.AsEnumerable());
		}

		/// <summary>
		/// Add page processors to spider.
		/// </summary>
		/// <param name="processors">页面解析器</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPageProcessors(IEnumerable<IPageProcessor> processors)
		{
			if (processors == null)
			{
				throw new ArgumentNullException($"{nameof(processors)} should not be null.");
			}
			if (processors.Count() > 0)
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
		/// Add a pipeline for Spider
		/// </summary>
		/// <param name="pipeline">数据管道</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPipeline(IPipeline pipeline)
		{
			return AddPipelines(pipeline);
		}

		/// <summary>
		/// Set pipelines for Spider
		/// </summary>
		/// <param name="pipelines">数据管道</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPipelines(params IPipeline[] pipelines)
		{
			if (pipelines == null)
			{
				throw new ArgumentNullException($"{nameof(pipelines)} should not be null.");
			}
			return AddPipelines(pipelines.AsEnumerable());
		}

		/// <summary>
		/// Set pipelines for Spider
		/// </summary>
		/// <param name="pipelines">数据管道</param>
		/// <returns>爬虫</returns>
		public virtual Spider AddPipelines(IEnumerable<IPipeline> pipelines)
		{
			if (pipelines == null)
			{
				throw new ArgumentNullException($"{nameof(pipelines)} should not be null.");
			}
			if (pipelines.Count() > 0)
			{
				CheckIfRunning();
				foreach (var pipeline in pipelines)
				{
					if (pipeline != null)
					{
						_pipelines.Add(pipeline);
					}
				}
			}
			return this;
		}

		/// <summary>
		/// Run spider.
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void Execute(params string[] arguments)
		{
			ValidateSettings();

			if (_inited || Status == Status.Running)
			{
				Logger.Warning("Crawler is running...");
				return;
			}

			InitComponents(arguments);

			if (arguments.Any(a => a?.ToLower() == "notrealrun"))
			{
				return;
			}

			StartTime = DateTime.Now;
			Status = Status.Running;
			_exited = false;

			ReportStatus();

			while (Status == Status.Running || Status == Status.Paused)
			{
				// 暂停则一直停在此处
				if (Status == Status.Paused)
				{
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
					while (Status == Status.Running)
					{
						// 从队列中取出一个请求
						Request request = Scheduler.Poll();

						// 如果队列中没有需要处理的请求, 则开始等待, 一直到设定的 EmptySleepTime 结束, 则认为爬虫应该结束了
						if (request == null)
						{
							if (waitCount > _waitCountLimit && ExitWhenComplete)
							{
								Status = Status.Finished;
								OnCompleted?.Invoke(this);
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
								Logger.Error($"Crawler {request.Url} failed: {e}.");
							}
							finally
							{
								if (request.Proxy != null)
								{
									var statusCode = request.StatusCode;
									Site.HttpProxyPool.ReturnProxy(request.Proxy, statusCode ?? HttpStatusCode.Found);
								}

								_requestedCount.Inc();

								if (_requestedCount.Value % _monitorReportInterval == 0)
								{
									ReportStatus();
									CheckExitSignal();
								}
							}
						}
					}

					SafeDestroy(downloader);
				});
			}
			string msg = Status != Status.Finished ? "Crawl terminated" : "Crawl complete";
			EndTime = DateTime.Now;

			ReportStatus();
			OnClose();

			Logger.Information($"{msg}, cost: {(EndTime - StartTime).TotalSeconds} seconds.");
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
			bool isRunning = Status == Status.Running;
			if (!isRunning)
			{
				Logger.Warning(Identity, "Crawler is not running.");
			}
			else
			{
				Status = Status.Paused;
				Logger.Information(Identity, "Stop running...");
			}
			action?.Invoke();
		}

		/// <summary>
		/// Contiune spider if spider is paused.
		/// </summary>
		public void Contiune()
		{
			if (Status == Status.Paused)
			{
				Status = Status.Running;
				Logger.Warning("Continue...");
			}
			else
			{
				Logger.Information("Crawler was not paused, can not continue...");
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
			if (Status == Status.Running || Status == Status.Paused)
			{
				Status = Status.Exited;
				Logger.Information("Exit...");
				return;
			}
			Logger.Warning(Identity, "Crawler is not running.");
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
		protected void ValidateSettings()
		{
			if (Site.RemoveOutboundLinks && (Site.Domains == null || Site.Domains.Length == 0))
			{
				throw new ArgumentException($"When you want remove outbound links, the domains should not be null or empty.");
			}
		}

		/// <summary>
		/// Init component of spider.
		/// </summary>
		/// <param name="arguments"></param>
		protected virtual void InitComponents(params string[] arguments)
		{
			PrintInfo.Print();

			Logger.Information("Build internal component...");

#if !NETSTANDARD // 开启多线程支持
			ServicePointManager.DefaultConnectionLimit = 1000;
#endif

			InitSite();

			InitDownloader();

			InitScheduler(arguments);

			if (_pageProcessors == null || _pageProcessors.Count == 0)
			{
				throw new SpiderException("Count of PageProcessor is zero");
			}

			InitPipelines(arguments);

			InitCloseSignals();

			InitMonitor();

			InitErrorRequestsLog();

			BuildStartUrlBuilders(arguments);

			PushStartRequestsToScheduler();

			_monitorReportInterval = CalculateMonitorReportInterval();

			if (!Console.IsInputRedirected)
			{
				Console.CancelKeyPress += ConsoleCancelKeyPress;
			}

			_waitCountLimit = EmptySleepTime / WaitInterval;

			_inited = true;
		}

		/// <summary>
		/// Get the default pipeline when user forget set a pepeline to spider.
		/// </summary>
		/// <returns>数据管道</returns>
		protected virtual IPipeline GetDefaultPipeline()
		{
			return new ConsolePipeline();
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

			SafeDestroyScheduler();
			SafeDestroy(_pageProcessors);
			SafeDestroy(Downloader);

			SafeDestroy(Site.HttpProxyPool);
			SafeDestroy(_errorRequestStreamWriter);
			SafeDestroy(_identityMmf);
			SafeDestroy(_taskIdMmf);
		}

		/// <summary>
		/// Event when spider on complete.
		/// </summary>
		protected virtual void SafeDestroyScheduler()
		{
			IsCompleted = Scheduler.LeftRequestsCount == 0;
			if (ClearSchedulerAfterCompleted && IsCompleted)
			{
				Scheduler.Dispose();
			}
		}

		/// <summary>
		/// Record error request.
		/// </summary>
		/// <param name="request"></param>
		protected void OnError(Request request)
		{
			lock (this)
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
			OnRequestSucceeded?.Invoke(request);
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

				page = downloader.Download(request, this).Result;

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

					foreach (var processor in _pageProcessors)
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
				if (page != null)
				{
					OnError(page.Request);
				}
				Logger.Error($"Should not catch download exception: {request.Url}.");
			}
			catch (Exception e)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = Site.AddToCycleRetry(request);
				}
				if (page != null)
				{
					OnError(page.Request);
				}
				Logger.Error($"Extract {request.Url} failed, please check your pipeline: {e}.");
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
					if (SkipTargetUrlsWhenResultIsEmpty)
					{
						Logger.Warning($"Skip {request.Url} because extract 0 result.");
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
							page = Site.AddToCycleRetry(request);
							if (page != null && page.Retry)
							{
								RetriedTimes.Inc();
								ExtractAndAddRequests(page);
							}
							Logger.Warning($"Download {request.Url} success, retry becuase extract 0 result.");
						}
						else
						{
							Logger.Warning($"Download {request.Url} success, will not retry because Site.CycleRetryTimes is 0.");
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

				int countOfResults = 0, effectedRows = 0;

				ResultItems[] resultItems = null;
				if (PipelineCachedSize == 1)
				{
					resultItems = new[] { page.ResultItems };
				}
				else
				{
					lock (this)
					{
						_cached.Add(page.ResultItems);
						if (_cached.Count >= PipelineCachedSize)
						{
							resultItems = _cached.ToArray();
							_cached.Clear();
						}
					}
				}

				foreach (IPipeline pipeline in Pipelines)
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
						Logger.Error($"Execute pipeline failed: {e}");
					}
				}

				foreach (var item in resultItems)
				{
					countOfResults += item.Request.CountOfResults.HasValue ? item.Request.CountOfResults.Value : 0;
					effectedRows += item.Request.EffectedRows.HasValue ? item.Request.EffectedRows.Value : 0;
				}

				Logger.Information($"Crawl: {request.Url} success, results: { request.CountOfResults}, effectedRow: {request.EffectedRows}.");

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
			if (page.Request.NextDepth <= Scheduler.Depth && page.TargetRequests != null &&
				page.TargetRequests.Count > 0)
			{
				foreach (Request request in page.TargetRequests)
				{
					Scheduler.Push(request);
				}
			}
		}

		/// <summary>
		/// Check whether spider is running.
		/// </summary>
		protected void CheckIfRunning()
		{
			if (Status == Status.Running)
			{
				throw new SpiderException("Spider is running");
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
			_cached = new List<ResultItems>(PipelineCachedSize);

			PipelineRetryTimes = PipelineRetryTimes <= 0 ? 1 : PipelineRetryTimes;

			_pipelineRetryPolicy = Policy.Handle<Exception>().Retry(PipelineRetryTimes, (ex, count) =>
			{
				Logger.Error($"Execute pipeline failed [{count}]: {ex}");
			});

			if (_pipelines == null || _pipelines.Count == 0)
			{
				var defaultPipeline = GetDefaultPipeline();
				if (defaultPipeline != null)
				{
					_pipelines.Add(defaultPipeline);
				}
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
					Logger.Error(e.ToString());
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

		private void BuildStartUrlBuilders(params string[] arguments)
		{
			if (_startUrlsBuilders != null && _startUrlsBuilders.Count > 0 && IfRequireBuildStartUrlsBuilders(arguments))
			{
				try
				{
					for (int i = 0; i < _startUrlsBuilders.Count; ++i)
					{
						var builder = _startUrlsBuilders[i];
						Logger.Information($"Add start urls to scheduler via builder[{i + 1}].");
						builder.Build(Site);
					}
				}
				finally
				{
					BuildStartUrlsBuildersCompleted();
				}
			}
		}

		private void PushStartRequestsToScheduler()
		{
			if (Site.StartRequests != null && Site.StartRequests.Count() > 0)
			{
				Logger.Information($"Add start urls to scheduler, count {Site.StartRequests.Count()}.");

				if (!Scheduler.IsDistributed)
				{
					foreach (var request in Site.StartRequests)
					{
						Scheduler.Push(request);
					}
				}
				else
				{
					Scheduler.Import(new HashSet<Request>(Site.StartRequests));
					// 释放本地内存
					Site.ClearStartRequests();
				}
			}
			else
			{
				Logger.Information("Add start urls to scheduler, count 0.");
			}
		}

		private void InitCloseSignals()
		{
			if (Env.IsWindows)
			{
				_identityMmf = MemoryMappedFile.CreateOrOpen(Identity, 1, MemoryMappedFileAccess.ReadWrite);
				using (MemoryMappedViewStream stream = _identityMmf.CreateViewStream())
				{
					var writer = new BinaryWriter(stream);
					writer.Write(false);
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
				_closeSignalFiles[0] = Path.Combine(Env.BaseDirectory, $"{Identity}_cl");

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

		private void ReportStatus()
		{
			try
			{
				Monitor?.Report(Identity, TaskId, Status.ToString(),
						Monitorable.LeftRequestsCount,
						Monitorable.TotalRequestsCount,
						Monitorable.SuccessRequestsCount,
						Monitorable.ErrorRequestsCount,
						AvgDownloadSpeed,
						AvgProcessorSpeed,
						AvgPipelineSpeed,
						ThreadNum);
			}
			catch (Exception e)
			{
				Logger.Error($"Report status failed: {e}.");
			}
		}

		private void InitMonitor()
		{
			if (Monitor == null)
			{
				Monitor = string.IsNullOrWhiteSpace(Env.HubServiceUrl) ? new LogMonitor() : new HttpMonitor();
			}
		}

		/// <summary>
		/// 计算状态监控器每完成多少个Request则上报状态
		/// </summary>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		private int CalculateMonitorReportInterval()
		{
			var leftCount = Scheduler.LeftRequestsCount;
			if (leftCount > 10)
			{
				return 10;
			}

			if (leftCount > 5)
			{
				return 2;
			}

			return 1;
		}

		private void InitErrorRequestsLog()
		{
			_errorRequestsLogFile = FileUtil.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));

			try
			{
				_errorRequestStreamWriter = new StreamWriter(File.Open(_errorRequestsLogFile.FullName, FileMode.OpenOrCreate));
			}
			catch
			{
				_errorRequestsLogFile = FileUtil.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, $"errors.{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt"));
				_errorRequestStreamWriter = new StreamWriter(File.Open(_errorRequestsLogFile.FullName, FileMode.OpenOrCreate));
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
				CheckExitSignalByMMF();
			}
			else
			{
				CheckExitSignalByFile();
			}
		}

		private void CheckExitSignalByMMF()
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

		private void CheckExitSignalByFile()
		{
			if (File.Exists(_closeSignalFiles[0]) || File.Exists(_closeSignalFiles[1]))
			{
				Exit();
			}
		}
	}
}