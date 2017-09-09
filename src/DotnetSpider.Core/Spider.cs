using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Proxy;
using DotnetSpider.Core.Scheduler;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using NLog;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;

namespace DotnetSpider.Core
{
	/// <summary>
	/// A spider contains four modules: Downloader, Scheduler, PageProcessor and Pipeline. 
	/// </summary>
	public class Spider : ISpider, ISpeedMonitor, INamed, ITask
	{
		private static readonly object Locker = new object();
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		private readonly Site _site;
		private IScheduler _scheduler;
		private IDownloader _downloader = new HttpClientDownloader();
		private Task _monitorTask;
		private ICookieInjector _cookieInjector;
		private Status _realStat = Status.Init;
		private readonly List<ResultItems> _cached = new List<ResultItems>();
		private readonly List<IStartUrlBuilder> _startUrlBuilders = new List<IStartUrlBuilder>();
		private int _waitCountLimit = 1500;
		private bool _init;
		private FileInfo _errorRequestFile;
		private readonly object _avgDownloadTimeLocker = new object();
		private readonly object _avgProcessorTimeLocker = new object();
		private readonly object _avgPipelineTimeLocker = new object();
		private int _threadNum = 1;
		private int _deep = int.MaxValue;
		private bool _spawnUrl = true;
		private bool _skipWhenResultIsEmpty;
		private bool _retryWhenResultIsEmpty;
		private bool _exitWhenComplete = true;
		private int _emptySleepTime = 15000;
		private int _cachedSize = 1;
		private string _identity;

		protected readonly List<IPageProcessor> PageProcessors = new List<IPageProcessor>();
		protected List<IPipeline> Pipelines = new List<IPipeline>();

		protected DateTime StartTime { get; private set; }

		protected DateTime EndTime { get; private set; } = DateTime.MinValue;

		protected int WaitInterval { get; } = 10;

		public string Identity
		{
			get => _identity;
			set
			{
				CheckIfRunning();

				if (string.IsNullOrEmpty(value) || value.Length > 120)
				{
					throw new ArgumentException("Length of Identity should less than 100.");
				}

				_identity = value;
			}
		}

		public string Name { get; set; }

		public Site Site => _site;

		public bool IsComplete { get; private set; }

		public AutomicLong RetriedTimes { get; } = new AutomicLong();

		public Status Stat { get; private set; } = Status.Init;

		public event Action<Request> OnSuccess;
		public event Action<Spider> OnClosing;
		public event Action<Spider> OnComplete;
		public event Action<Spider> OnClosed;

		public bool ClearSchedulerAfterComplete { get; set; } = true;

		public IMonitor Monitor { get; set; }

		public string TaskId { get; set; }

		public long AvgDownloadSpeed { get; private set; }

		public long AvgProcessorSpeed { get; private set; }

		public long AvgPipelineSpeed { get; private set; }

		public int StatusReportInterval { get; set; } = 5000;

		public int PipelineRetryTimes { get; set; } = 1;

		public IScheduler Scheduler
		{
			get => _scheduler;
			set
			{
				CheckIfRunning();
				_scheduler = value;
			}
		}

		public int CachedSize
		{
			get => _cachedSize;
			set
			{
				CheckIfRunning();
				_cachedSize = value;
			}
		}

		public IRedialExecutor RedialExecutor
		{
			get => NetworkCenter.Current.Executor;
			set
			{
				CheckIfRunning();
				NetworkCenter.Current.Executor = value;
			}
		}

		public IDownloader Downloader
		{
			get => _downloader;
			set
			{
				CheckIfRunning();
				_downloader = value;
			}
		}

		public ICookieInjector CookieInjector
		{
			get => _cookieInjector;
			set
			{
				CheckIfRunning();
				_cookieInjector = value;
			}
		}

		public int EmptySleepTime
		{
			get => _emptySleepTime;
			set
			{
				CheckIfRunning();

				if (value >= 1000)
				{
					_emptySleepTime = value;
					_waitCountLimit = value / WaitInterval;
				}
				else
				{
					throw new SpiderException("Sleep time should be large than 1000.");
				}
			}
		}

		public bool ExitWhenComplete
		{
			get => _exitWhenComplete;
			set
			{
				CheckIfRunning();
				_exitWhenComplete = value;
			}
		}

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

		public int Deep
		{
			get => _deep;
			set
			{
				CheckIfRunning();
				_deep = value;
			}
		}

		public bool SpawnUrl
		{
			get => _spawnUrl;
			set
			{
				CheckIfRunning();
				_spawnUrl = value;
			}
		}

		public bool SkipWhenResultIsEmpty
		{
			get => _skipWhenResultIsEmpty;
			set
			{
				CheckIfRunning();
				_skipWhenResultIsEmpty = value;
			}
		}

		public bool RetryWhenResultIsEmpty
		{
			get => _retryWhenResultIsEmpty;
			set
			{
				CheckIfRunning();
				_retryWhenResultIsEmpty = value;
			}
		}

		public ReadOnlyCollection<IPageProcessor> ReadOnlyPageProcessors => PageProcessors.AsReadOnly();

		public ReadOnlyCollection<IPipeline> ReadOnlyPipelines => Pipelines.AsReadOnly();

		public ReadOnlyCollection<IStartUrlBuilder> StartUrlBuilders => _startUrlBuilders.AsReadOnly();

		public IMonitorable Monitorable => Scheduler;

		/// <summary>
		/// Create a spider with pageProcessor.
		/// </summary>
		/// <param name="site"></param>
		/// <param name="pageProcessors"></param>
		/// <returns></returns>
		public static Spider Create(Site site, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, Guid.NewGuid().ToString("N"), new QueueDuplicateRemovedScheduler(),
				pageProcessors);
		}

		/// <summary>
		/// Create a spider with pageProcessor and scheduler
		/// </summary>
		/// <param name="site"></param>
		/// <param name="pageProcessors"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static Spider Create(Site site, IScheduler scheduler, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, Guid.NewGuid().ToString("N"), scheduler, pageProcessors);
		}

		public static Page AddToCycleRetry(Request request, Site site, bool resultIsEmpty = false)
		{
			Page page = new Page(request, null)
			{
				ContentType = site.ContentType
			};

			if (!resultIsEmpty)
			{
				dynamic cycleTriedTimesObject = request.GetExtra(Request.CycleTriedTimes);
				if (cycleTriedTimesObject == null)
				{
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.CycleTriedTimes, 1), false);
				}
				else
				{
					int cycleTriedTimes = (int)cycleTriedTimesObject;
					cycleTriedTimes++;
					if (cycleTriedTimes > site.CycleRetryTimes)
					{
						return null;
					}
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.CycleTriedTimes, cycleTriedTimes), false);
				}
				page.Retry = true;
				return page;
			}
			else
			{
				dynamic cycleTriedTimesObject = request.GetExtra(Request.ResultIsEmptyTriedTimes);
				if (cycleTriedTimesObject == null)
				{
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.ResultIsEmptyTriedTimes, 1), false);
				}
				else
				{
					int cycleTriedTimes = (int)cycleTriedTimesObject;
					cycleTriedTimes++;
					if (cycleTriedTimes > site.CycleRetryTimes)
					{
						return null;
					}
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.ResultIsEmptyTriedTimes, cycleTriedTimes), false);
				}
				page.Retry = true;
				return page;
			}
		}

		/// <summary>
		/// Create a spider with pageProcessor and scheduler
		/// </summary>
		/// <param name="site"></param>
		/// <param name="identify"></param>
		/// <param name="pageProcessors"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static Spider Create(Site site, string identify, IScheduler scheduler, params IPageProcessor[] pageProcessors)
		{
			return new Spider(site, identify, scheduler, pageProcessors);
		}

		protected Spider()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
#endif
			LogCenter.InitLogCenter();
		}

		public Spider AddStartUrlBuilder(IStartUrlBuilder builder)
		{
			_startUrlBuilders.Add(builder);
			return this;
		}

		/// <summary>
		/// Set startUrls of Spider. 
		/// Prior to startUrls of Site.
		/// </summary>
		/// <param name="startUrls"></param>
		/// <returns></returns>
		public Spider AddStartUrls(IList<string> startUrls)
		{
			CheckIfRunning();
			Site.StartRequests.AddRange(UrlUtils.ConvertToRequests(startUrls, 1));
			return this;
		}

		/// <summary>
		/// Set startUrls of Spider. 
		/// Prior to startUrls of Site.
		/// </summary>
		/// <param name="startRequests"></param>
		/// <returns></returns>
		public Spider AddStartRequests(IList<Request> startRequests)
		{
			CheckIfRunning();
			Site.StartRequests.AddRange(startRequests);
			return this;
		}

		/// <summary>
		/// Add urls to crawl.
		/// </summary>
		/// <param name="urls"></param>
		/// <returns></returns>
		public Spider AddStartUrl(params string[] urls)
		{
			foreach (string url in urls)
			{
				AddStartRequest(new Request(url, null));
			}
			return this;
		}

		/// <summary>
		/// Add urls to crawl.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="extras"></param>
		/// <returns></returns>
		public Spider AddStartUrl(string url, Dictionary<string, dynamic> extras)
		{
			AddStartRequest(new Request(url, extras));
			return this;
		}

		public Spider AddStartUrl(ICollection<string> urls)
		{
			foreach (string url in urls)
			{
				AddStartRequest(new Request(url, null));
			}

			return this;
		}

		/// <summary>
		/// Add urls with information to crawl.
		/// </summary>
		/// <param name="requests"></param>
		/// <returns></returns>
		public Spider AddStartRequest(params Request[] requests)
		{
			CheckIfRunning();
			Site.StartRequests.AddRange(requests);
			return this;
		}

		/// <summary>
		/// Add a pipeline for Spider
		/// </summary>
		/// <param name="pipeline"></param>
		/// <returns></returns>
		public virtual Spider AddPipeline(IPipeline pipeline)
		{
			if (pipeline != null)
			{
				CheckIfRunning();
				Pipelines.Add(pipeline);
			}
			return this;
		}

		public virtual Spider AddPageProcessor(params IPageProcessor[] processors)
		{
			if (processors != null && processors.Length > 0)
			{
				CheckIfRunning();
				foreach (var processor in processors)
				{
					PageProcessors.Add(processor);
				}
			}
			return this;
		}

		/// <summary>
		/// Set pipelines for Spider
		/// </summary>
		/// <param name="pipelines"></param>
		/// <returns></returns>
		public virtual Spider AddPipelines(IList<IPipeline> pipelines)
		{
			CheckIfRunning();
			foreach (var pipeline in pipelines)
			{
				AddPipeline(pipeline);
			}
			return this;
		}

		public IList<IPipeline> GetPipelines()
		{
			return ReadOnlyPipelines;
		}

		/// <summary>
		/// Clear the pipelines set
		/// </summary>
		/// <returns></returns>
		public Spider ClearPipeline()
		{
			Pipelines = new List<IPipeline>();
			return this;
		}

		public virtual void Run(params string[] arguments)
		{
			if (Stat == Status.Running)
			{
				Logger.MyLog(Identity, "Crawler is running...", LogLevel.Warn);
				return;
			}

			CheckIfSettingsCorrect();

#if !NET_CORE // 开启多线程支持
			ServicePointManager.DefaultConnectionLimit = 1000;
#endif

			InitComponent(arguments);

			Monitorable.IsExited = false;

			_monitorTask = Task.Factory.StartNew(() =>
			{
				while (!Monitorable.IsExited)
				{
					ReportStatus();
					Thread.Sleep(StatusReportInterval);
				}
				ReportStatus();
			});

			if (arguments.Contains("running-test"))
			{
				_scheduler.IsExited = true;
				return;
			}

			if (StartTime == DateTime.MinValue)
			{
				StartTime = DateTime.Now;
			}

			Stat = Status.Running;
			_realStat = Status.Running;

			while (Stat == Status.Running || Stat == Status.Stopped)
			{
				if (Stat == Status.Stopped)
				{
					_realStat = Status.Stopped;
					Thread.Sleep(50);
					continue;
				}

				Parallel.For(0, ThreadNum, new ParallelOptions
				{
					MaxDegreeOfParallelism = ThreadNum
				}, i =>
				{
					int waitCount = 0;
					//bool firstTask = true;

					using (var downloader = Downloader.Clone(this))
					{
						while (Stat == Status.Running)
						{
							Request request = Scheduler.Poll();

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
								waitCount = 0;

								try
								{
									Stopwatch sw = new Stopwatch();
									ProcessRequest(sw, request, downloader);
									Thread.Sleep(Site.SleepTime);
								}
								catch (Exception e)
								{
									OnError(request);
									Logger.MyLog(Identity, $"Crawler failed: {request.Url}.", LogLevel.Error, e);
								}
								finally
								{
									if (request.GetExtra(Request.Proxy) != null)
									{
										var statusCode = request.StatusCode;
										Site.ReturnHttpProxy(request.GetExtra(Request.Proxy) as UseSpecifiedUriWebProxy,
											statusCode ?? HttpStatusCode.Found);
									}
								}

								//if (firstTask)
								//{
								//	Thread.Sleep(3000);
								//	firstTask = false;
								//}
							}
						}
					}
				});

				Thread.Sleep(3000);
			}

			EndTime = DateTime.Now;
			_realStat = Status.Exited;

			OnClose();

			Logger.MyLog(Identity, "Waiting for monitor exit.", LogLevel.Info);
			_monitorTask.Wait(5000);

			OnClosing?.Invoke(this);

			var msg = Stat == Status.Finished ? "Crawl complete" : "Crawl terminated";
			Logger.MyLog(Identity, $"{msg}, cost: {(EndTime - StartTime).TotalSeconds} seconds.", LogLevel.Info);

			OnClosed?.Invoke(this);
		}

		public static void PrintInfo()
		{
			lock (Locker)
			{
				var key = "_DotnetSpider_Info";

#if !NET_CORE
				var isPrinted = AppDomain.CurrentDomain.GetData(key) != null;
#else
				AppContext.TryGetSwitch(key, out var isPrinted);
#endif
				if (!isPrinted)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("=================================================================");
					Console.WriteLine("== DotnetSpider is an open source crawler developed by C#      ==");
					Console.WriteLine("== It's multi thread, light weight, stable and high performce  ==");
					Console.WriteLine("== Support storage data to file, mysql, mssql, mongodb etc     ==");
					Console.WriteLine("== License: LGPL3.0                                            ==");
					Console.WriteLine("== Author: zlzforever@163.com                                  ==");
					Console.WriteLine("=================================================================");
					Console.ForegroundColor = ConsoleColor.White;
#if !NET_CORE
					AppDomain.CurrentDomain.SetData(key, "True");
#else
					AppContext.SetSwitch(key, true);
#endif
				}
				Console.WriteLine();
				Console.WriteLine("=================================================================");
			}
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() => { Run(arguments); });
		}

		public void Pause(Action action = null)
		{
			if (Stat != Status.Running)
			{
				Logger.MyLog(Identity, "Crawler is not running.", LogLevel.Warn);
				return;
			}
			Stat = Status.Stopped;
			Logger.MyLog(Identity, "Stop running...", LogLevel.Warn);
			if (action != null)
			{
				Task.Factory.StartNew(() =>
				{
					while (_realStat != Status.Stopped)
					{
						Thread.Sleep(100);
					}
					action();
				});
			}
		}

		public void Contiune()
		{
			if (_realStat == Status.Stopped)
			{
				Stat = Status.Running;
				_realStat = Status.Running;
				Logger.MyLog(Identity, "Continue...", LogLevel.Warn);
			}
			else
			{
				Logger.MyLog(Identity, "Crawler is not pause, can not continue...", LogLevel.Warn);
			}
		}

		public void Exit(Action action = null)
		{
			if (Stat == Status.Running || Stat == Status.Stopped)
			{
				Stat = Status.Exited;
				Logger.MyLog(Identity, "Exit...", LogLevel.Warn);
				return;
			}
			Logger.MyLog(Identity, "Crawler is not running.", LogLevel.Warn);
			if (action != null)
			{
				Task.Factory.StartNew(() =>
				{
					while (_realStat != Status.Exited)
					{
						Thread.Sleep(100);
					}
					action();
				});
			}
		}

		public void Dispose()
		{
			CheckIfRunning();

			int i = 0;
			while (!_scheduler.IsExited)
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

		protected Spider(Site site) : this()
		{
			_site = site ?? throw new SpiderException("Site should not be null.");
		}

		/// <summary>
		/// Create a spider with pageProcessor.
		/// </summary>
		/// <param name="site"></param>
		/// <param name="identity"></param>
		/// <param name="pageProcessors"></param>
		/// <param name="scheduler"></param>
		protected Spider(Site site, string identity, IScheduler scheduler, params IPageProcessor[] pageProcessors) : this()
		{
			Identity = identity;

			if (pageProcessors != null)
			{
				PageProcessors = pageProcessors.ToList();
			}
			_site = site;

			Scheduler = scheduler;

			if (_site == null)
			{
				_site = new Site();
			}

			CheckIfSettingsCorrect();
		}

		protected void CheckIfSettingsCorrect()
		{
			Identity = (string.IsNullOrWhiteSpace(Identity) || string.IsNullOrEmpty(Identity))
				? Encrypt.Md5Encrypt(Guid.NewGuid().ToString())
				: Identity;

			if (Identity.Length > 100)
			{
				throw new SpiderException("Length of Identity should less than 100.");
			}

			if (PageProcessors == null || PageProcessors.Count == 0)
			{
				throw new SpiderException("Count of PageProcessor is zero.");
			}

			Site.Accept = Site.Accept ?? "application/json, text/javascript, */*; q=0.01";
			Site.UserAgent = Site.UserAgent ??
							 "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
			if (!Site.Headers.ContainsKey("Accept-Language"))
			{
				Site.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
			}

			foreach (var processor in PageProcessors)
			{
				processor.Site = Site;
			}

			Scheduler = Scheduler ?? new QueueDuplicateRemovedScheduler();
			Downloader = Downloader ?? new HttpClientDownloader();
		}

		protected virtual void PreInitComponent(params string[] arguments)
		{
			Monitor = Monitor ?? new NLogMonitor();
		}

		protected virtual void AfterInitComponent(params string[] arguments)
		{
		}

		protected virtual void InitComponent(params string[] arguments)
		{
			PrintInfo();

			if (_init)
			{
				return;
			}

			Logger.MyLog(Identity, "Build crawler...", LogLevel.Info);

			if (Pipelines == null || Pipelines.Count == 0)
			{
				var defaultPipeline = GetDefaultPipeline();
				if (defaultPipeline == null)
				{
					throw new SpiderException("Pipelines should not be null.");
				}
				else
				{
					Pipelines.Add(defaultPipeline);
				}
			}

			PreInitComponent(arguments);

			Monitor.Identity = Identity;

			CookieInjector?.Inject(this, false);

			Scheduler.Init(this);

			_errorRequestFile = BasePipeline.PrepareFile(Path.Combine(Environment.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));

			Console.CancelKeyPress += ConsoleCancelKeyPress;

			foreach (var pipeline in Pipelines)
			{
				pipeline.InitPipeline(this);
			}

			if (Site.StartRequests != null && Site.StartRequests.Count > 0)
			{
				Logger.MyLog(Identity, $"Add start urls to scheduler, count {Site.StartRequests.Count}.", LogLevel.Info);
				if ((Scheduler is QueueDuplicateRemovedScheduler) || (Scheduler is PriorityScheduler))
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
				Logger.MyLog(Identity, "Add start urls to scheduler, count 0.", LogLevel.Info);
			}

			_waitCountLimit = EmptySleepTime / WaitInterval;

			AfterInitComponent(arguments);

			_init = true;
		}

		protected virtual IPipeline GetDefaultPipeline()
		{
			return null;
		}

		protected void OnClose()
		{
			foreach (IPipeline pipeline in Pipelines)
			{
				pipeline.Process(_cached.ToArray());
				SafeDestroy(pipeline);
			}

			SafeDestroy(Scheduler);
			SafeDestroy(PageProcessors);
			SafeDestroy(Downloader);

			Site.HttpProxyPool?.Dispose();
		}

		protected virtual void _OnComplete()
		{
			IsComplete = Scheduler.LeftRequestsCount == 0;
			if (ClearSchedulerAfterComplete && IsComplete)
			{
				Scheduler.Clear();
			}
		}

		protected void OnError(Request request)
		{
			lock (Locker)
			{
				File.AppendAllText(_errorRequestFile.FullName, $"{request}{System.Environment.NewLine}", Encoding.UTF8);
			}
			Scheduler.IncreaseErrorCount();
		}

		protected void _OnSuccess(Request request)
		{
			Scheduler.IncreaseSuccessCount();
			OnSuccess?.Invoke(request);
		}

		protected void ProcessRequest(Stopwatch sw, Request request, IDownloader downloader)
		{
			Page page = null;

			try
			{
				sw.Reset();
				sw.Start();

				page = downloader.Download(request, this);

				sw.Stop();
				CalculateDownloadSpeed(sw.ElapsedMilliseconds);

				if (page == null || page.SkipRequest)
				{
					return;
				}

				if (page.Exception == null)
				{
					sw.Reset();
					sw.Start();

					foreach (var processor in PageProcessors)
					{
						processor.Process(page);
					}

					sw.Stop();
					CalculateProcessorSpeed(sw.ElapsedMilliseconds);
				}
				else
				{
					OnError(page.Request);
				}
			}
			catch (DownloadException de)
			{
				if (page != null) OnError(page.Request);
				Logger.MyLog(Identity, $"Should not catch download exception: {request.Url}.", LogLevel.Error, de);
			}
			catch (XPathException xe)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				if (page != null) OnError(page.Request);
				Logger.MyLog(Identity, $"Extract data failed: {request.Url}, selector: {xe.Message}, maybe you should set SelectorType to Json.", LogLevel.Error, xe);
			}
			catch (JsonReaderException je)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				if (page != null) OnError(page.Request);
				Logger.MyLog(Identity, $"Extract data failed: {request.Url}, selector: {je.Message}, maybe you should set SelectorType to XPATH.", LogLevel.Error, je);
			}
			catch (Exception e)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				if (page != null) OnError(page.Request);
				Logger.MyLog(Identity, $"Extract data failed: {request.Url}, please check your extractor: {e.Message}.", LogLevel.Error, e);
			}

			if (page == null)
			{
				return;
			}
			// 此处是用于需要循环本身的场景, 不能使用本身Request的原因是Request的尝试次数计算问题
			if (page.Retry)
			{
				RetriedTimes.Inc();
				ExtractAndAddRequests(page, true);
				return;
			}

			if (!page.SkipTargetUrls && !(SkipWhenResultIsEmpty && page.ResultItems.IsSkip))
			{
				ExtractAndAddRequests(page, SpawnUrl);
			}

			if (page.Exception == null)
			{
				sw.Reset();
				sw.Start();

				if (!page.ResultItems.IsSkip)
				{
					if (CachedSize == 1)
					{
						foreach (IPipeline pipeline in Pipelines)
						{
							RetryExecutor.Execute(PipelineRetryTimes, () =>
							{
								pipeline.Process(page.ResultItems);
							});
						}
					}
					else
					{
						lock (Locker)
						{
							_cached.Add(page.ResultItems);

							if (_cached.Count >= CachedSize)
							{
								var items = _cached.ToArray();
								_cached.Clear();
								foreach (IPipeline pipeline in Pipelines)
								{
									pipeline.Process(items);
								}
							}
						}
					}
					Logger.MyLog(Identity, $"Crawl: {request.Url} success.", LogLevel.Info);
				}
				else
				{
					if (RetryWhenResultIsEmpty)
					{
						if (Site.CycleRetryTimes > 0)
						{
							page = AddToCycleRetry(request, Site, true);
							if (page != null && page.Retry)
							{
								RetriedTimes.Inc();
								ExtractAndAddRequests(page, true);
							}
							Logger.MyLog(Identity, $"Download: {request.Url} success, extract 0, retry.", LogLevel.Warn);
						}
						else
						{
							Logger.MyLog(Identity, $"Download: {request.Url} success, extract 0.", LogLevel.Warn);
						}
					}
					else
					{
						Logger.MyLog(Identity, $"Download: {request.Url} success, extract 0.", LogLevel.Warn);
					}
				}

				sw.Stop();
				CalculatePipelineSpeed(sw.ElapsedMilliseconds);

				_OnSuccess(request);
			}
		}

		protected void ExtractAndAddRequests(Page page, bool spawnUrl)
		{
			if (spawnUrl && page.Request.NextDepth <= Deep && page.TargetRequests != null &&
				page.TargetRequests.Count > 0)
			{
				foreach (Request request in page.TargetRequests)
				{
					Scheduler.Push(request);
				}
			}
		}

		protected void CheckIfRunning()
		{
			if (Stat == Status.Running)
			{
				throw new SpiderException("Spider is running.");
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
					Logger.MyLog(Identity, e.ToString(), LogLevel.Error);
				}
			}
		}

		private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Exit();
			while (!_scheduler.IsExited)
			{
				Thread.Sleep(1500);
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

		private void ReportStatus()
		{
			try
			{
				Monitor.Report(Stat.ToString(),
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
				Logger.MyLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
			}
		}
	}
}