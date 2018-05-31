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
using NLog;
using System.Runtime.CompilerServices;
using System.Reflection;
using Polly;
using Polly.Retry;

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
        private Task _monitorTask;
        private ICookieInjector _cookieInjector;
        private Status _realStat = Status.Init;
        private readonly List<ResultItems> _cached = new List<ResultItems>();
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
        private string[] _closeSignalFiles;

        protected virtual bool IfRequireInitStartRequests(string[] arguments)
        {
            return true;
        }

        protected virtual void InitStartRequestsFinished()
        {
        }

        /// <summary>
        /// Storage all processors for spider.
        /// </summary>
        protected readonly List<IPageProcessor> PageProcessors = new List<IPageProcessor>();

        /// <summary>
        /// Storage all pipelines for spider.
        /// </summary>
        protected List<IPipeline> Pipelines = new List<IPipeline>();

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

                if (string.IsNullOrEmpty(value) || value.Length > 120)
                {
                    throw new ArgumentException("Length of Identity should less than 100.");
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
        /// Event of crawler on closing.
        /// </summary>
        public event Action<Spider> OnClosing;

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
        public readonly List<IStartUrlBuilder> StartUrlBuilders = new List<IStartUrlBuilder>();

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
        /// Interface to inject cookie.
        /// </summary>
        public ICookieInjector CookieInjector
        {
            get => _cookieInjector;
            set
            {
                CheckIfRunning();
                _cookieInjector = value;
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
        /// <param name="site"></param>
        /// <param name="pageProcessors"></param>
        /// <returns></returns>
        public static Spider Create(Site site, params IPageProcessor[] pageProcessors)
        {
            return new Spider(site, Guid.NewGuid().ToString("N"), new QueueDuplicateRemovedScheduler(),
                pageProcessors);
        }

        /// <summary>
        /// Create a spider with pageProcessors and scheduler.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="pageProcessors"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static Spider Create(Site site, IScheduler scheduler, params IPageProcessor[] pageProcessors)
        {
            return new Spider(site, Guid.NewGuid().ToString("N"), scheduler, pageProcessors);
        }

        public static Page AddToCycleRetry(Request request, Site site)
        {
            Page page = new Page(request, null)
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
        /// Create a spider with pageProcessors and scheduler.
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
        /// Add start url builder to spider.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public Spider AddStartUrlBuilder(IStartUrlBuilder builder)
        {
            StartUrlBuilders.Add(builder);
            return this;
        }

        /// <summary>
        /// Add startUrls to spider. 
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
        /// Add start requests to spider. 
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
        /// Add urls to spider.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="extras">Extra properties of request.</param>
        /// <returns></returns>
        public Spider AddStartUrl(string url, Dictionary<string, dynamic> extras)
        {
            AddStartRequest(new Request(url, extras));
            return this;
        }

        /// <summary>
        /// Add start urls to spider.
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add page processors to spider.
        /// </summary>
        /// <param name="processors"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Used for testing.
        /// </summary>
        /// <returns>All pipelines of spider.</returns>
        public IList<IPipeline> GetPipelines()
        {
            return Pipelines.AsReadOnly();
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

        /// <summary>
        /// Run spider.
        /// </summary>
        /// <param name="arguments"></param>
        protected override void Execute(params string[] arguments)
        {
            if (Stat == Status.Running)
            {
                Logger.AllLog(Identity, "Crawler is running...", LogLevel.Warn);
                return;
            }

            CheckIfSettingsCorrect();

            if (_init)
            {
                return;
            }

            InitComponent(arguments);

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

            int monitorInterval = CalculateMonitorInterval();

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
                    int waitCount = 1;
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
                            waitCount = 1;

                            if (Scheduler.SuccessRequestsCount % monitorInterval == 0)
                            {
                                try
                                {
                                    ReportStatus();
                                }
                                catch (Exception e)
                                {
                                    Logger.AllLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
                                }
                            }

                            try
                            {
                                Stopwatch sw = new Stopwatch();
                                HandleRequest(sw, request, Downloader);
                                Thread.Sleep(Site.SleepTime);
                            }
                            catch (Exception e)
                            {
                                OnError(request);
                                Logger.AllLog(Identity, $"Crawler {request.Url} failed: {e}.", LogLevel.Error, e);
                            }
                            finally
                            {
                                if (request.Proxy != null)
                                {
                                    var statusCode = request.StatusCode;
                                    Site.ReturnHttpProxy(request.Proxy, statusCode ?? HttpStatusCode.Found);
                                }
                            }
                        }
                    }
                });
            }

            EndTime = DateTime.Now;
            _realStat = Status.Exited;

            ReportStatus();

            OnClose();

            OnClosing?.Invoke(this);

            var msg = Stat == Status.Finished ? "Crawl complete" : "Crawl terminated";
            Logger.AllLog(Identity, $"{msg}, cost: {(EndTime - StartTime).TotalSeconds} seconds.", LogLevel.Info);

            OnClosed?.Invoke(this);

            PrintInfo.PrintLine();
        }

        /// <summary>
        /// Pause spider.
        /// </summary>
        /// <param name="action"></param>
        public void Pause(Action action = null)
        {
            if (Stat != Status.Running)
            {
                Logger.AllLog(Identity, "Crawler is not running.", LogLevel.Warn);
                return;
            }
            Stat = Status.Stopped;
            Logger.AllLog(Identity, "Stop running...", LogLevel.Warn);
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

        /// <summary>
        /// Contiune spider if spider is paused.
        /// </summary>
        public void Contiune()
        {
            if (_realStat == Status.Stopped)
            {
                Stat = Status.Running;
                _realStat = Status.Running;
                Logger.AllLog(Identity, "Continue...", LogLevel.Warn);
            }
            else
            {
                Logger.AllLog(Identity, "Crawler was not paused, can not continue...", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Exit spider.
        /// </summary>
        /// <param name="action"></param>
        public void Exit(Action action = null)
        {
            if (Stat == Status.Running || Stat == Status.Stopped)
            {
                Stat = Status.Exited;
                Logger.AllLog(Identity, "Exit...", LogLevel.Warn);
                return;
            }
            Logger.AllLog(Identity, "Crawler is not running.", LogLevel.Warn);
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

        /// <summary>
        /// Dispose spider.
        /// </summary>
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
        /// Create a spider with site, identity, scheduler and pageProcessors.
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

        /// <summary>
        /// Check if all settings of spider are correct.
        /// </summary>
        protected void CheckIfSettingsCorrect()
        {
            Identity = (string.IsNullOrWhiteSpace(Identity) || string.IsNullOrEmpty(Identity))
                ? Encrypt.Md5Encrypt(Guid.NewGuid().ToString())
                : Identity;

            if (Identity.Length > 100)
            {
                throw new SpiderException("Length of Identity should less than 100.");
            }
        }

        /// <summary>
        /// Init component of spider.
        /// </summary>
        /// <param name="arguments"></param>
        protected virtual void InitComponent(params string[] arguments)
        {
            PrintInfo.Print();

            Logger.AllLog(Identity, "Build internal component...", LogLevel.Info);

#if !NET_CORE // 开启多线程支持
			ServicePointManager.DefaultConnectionLimit = 1000;
#endif
            InitSiteProperty();

            InitDownloader();

            InitScheduler(arguments);

            InitPageProcessor();

            InitPipelines(arguments);

            InitFileCloseSignals();

            InitMonitor();

            InjectCookie();

            Console.CancelKeyPress += ConsoleCancelKeyPress;

            _waitCountLimit = EmptySleepTime / WaitInterval;

            PrepareErrorRequestsLogFile();

            PipelineRetryTimes = PipelineRetryTimes <= 0 ? 1 : PipelineRetryTimes;

            _pipelineRetryPolicy = Policy.Handle<Exception>().Retry(PipelineRetryTimes, (ex, count) =>
            {
                Logger.Error($"Execute pipeline failed [{count}]: {ex}");
            });

            ExecuteStartUrlBuilders(arguments);

            PushStartRequestToScheduler();
        }

        /// <summary>
        /// Get the default pipeline when user forget set a pepeline to spider.
        /// </summary>
        /// <returns></returns>
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

            foreach (IPipeline pipeline in Pipelines)
            {
                if (containsData)
                {
                    pipeline.Process(_cached,this);
                }
                SafeDestroy(pipeline);
            }

            SafeDestroy(Scheduler);
            SafeDestroy(PageProcessors);
            SafeDestroy(Downloader);

            SafeDestroy(Site.HttpProxyPool);
            SafeDestroy(_errorRequestStreamWriter);
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
        /// <param name="sw"></param>
        /// <param name="request"></param>
        /// <param name="downloader"></param>
        protected void HandleRequest(Stopwatch sw, Request request, IDownloader downloader)
        {
            Page page = null;

            try
            {
                sw.Reset();
                sw.Start();

                page = downloader.Download(request, this);

                sw.Stop();
                CalculateDownloadSpeed(sw.ElapsedMilliseconds);

                if (page == null || page.Skip)
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
                Logger.AllLog(Identity, $"Should not catch download exception: {request.Url}.", LogLevel.Error, de);
            }
            catch (Exception e)
            {
                if (Site.CycleRetryTimes > 0)
                {
                    page = AddToCycleRetry(request, Site);
                }
                if (page != null) OnError(page.Request);
                Logger.AllLog(Identity, $"Extract {request.Url} failed, please check your pipeline: {e}.", LogLevel.Error, e);
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
                        Logger.AllLog(Identity, $"Skip {request.Url} because extract 0 result.", LogLevel.Warn);
                        _OnSuccess(request);
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
                            Logger.AllLog(Identity, $"Download {request.Url} success, retry becuase extract 0 result.", LogLevel.Warn);
                        }
                        else
                        {
                            Logger.AllLog(Identity, $"Download {request.Url} success, will not retry because Site.CycleRetryTimes is 0.", LogLevel.Warn);
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
                excutePipeline = true;
            }

            if (!excutePipeline)
            {
                return;
            }

            if (page.Exception == null)
            {
                sw.Reset();
                sw.Start();

                if (CachedSize == 1)
                {

                    _cached.Add(page.ResultItems);
                    foreach (IPipeline pipeline in Pipelines)
                    {
                        try
                        {
                            _pipelineRetryPolicy.Execute(() =>
                            {
                                pipeline.Process(_cached, this);
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.AllLog(Identity, $"Execute pipeline failed: {e}", LogLevel.Error);
                        }
                    }

                    _cached.Clear();
                }
                else
                {
                    lock (Locker)
                    {
                        _cached.Add(page.ResultItems);

                        if (_cached.Count >= CachedSize)
                        { 
                            foreach (IPipeline pipeline in Pipelines)
                            {
                                pipeline.Process(_cached, this);
                            }
                            _cached.Clear();
                        }
                    }
                }

                _OnSuccess(request);

                StringBuilder builder = new StringBuilder($"Crawl: {request.Url} success");
                if (request.CountOfResults != null)
                {
                    builder.Append($", results: { request.CountOfResults}");
                }
                if (request.EffectedRows != null)
                {
                    builder.Append($", effectedRow: {request.EffectedRows}");
                }
                builder.Append(".");
                Logger.AllLog(Identity, builder.ToString(), LogLevel.Info);

                sw.Stop();
                CalculatePipelineSpeed(sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Extract and add target urls to scheduler.
        /// </summary>
        /// <param name="page"></param>
        protected void ExtractAndAddRequests(Page page)
        {
            if (page.Request.NextDepth <= Deep && page.TargetRequests != null &&
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
            if (Stat == Status.Running)
            {
                throw new SpiderException("Spider is running.");
            }
        }

        protected virtual void InitScheduler(params string[] arguments)
        {
            Scheduler = Scheduler ?? new QueueDuplicateRemovedScheduler();
            Scheduler.Init(this);
            Monitorable.IsExited = false;
        }

        protected virtual void InitPageProcessor(params string[] arguments)
        {
            if (PageProcessors == null || PageProcessors.Count == 0)
            {
                throw new SpiderException("Count of PageProcessor is zero.");
            }
            foreach (var processor in PageProcessors)
            {
                processor.Site = Site;
            }
        }

        protected virtual void InitPipelines(params string[] arguments)
        {
            if (Pipelines == null || Pipelines.Count == 0)
            {
                var defaultPipeline = GetDefaultPipeline();
                if (defaultPipeline != null)
                {
                    Pipelines.Add(defaultPipeline);
                }
            }

            foreach (var pipeline in Pipelines)
            {
                pipeline.InitPipeline(this);
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
                    Logger.AllLog(Identity, e.ToString(), LogLevel.Error);
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

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            NetworkCenter.Current.Executor?.Dispose();
            Exit();
            while (!_scheduler.IsExited)
            {
                Thread.Sleep(1500);
            }
        }

        private void ExecuteStartUrlBuilders(params string[] arguments)
        {
            if (StartUrlBuilders != null && StartUrlBuilders.Count > 0 && IfRequireInitStartRequests(arguments))
            {
                try
                {
                    for (int i = 0; i < StartUrlBuilders.Count; ++i)
                    {
                        var builder = StartUrlBuilders[i];
                        Logger.AllLog(Identity, $"Add start urls to scheduler via builder[{i + 1}].", LogLevel.Info);
                        builder.Build(Site);
                    }
                }
                finally
                {
                    InitStartRequestsFinished();
                }
            }
        }

        private void PushStartRequestToScheduler()
        {
            if (Site.StartRequests != null && Site.StartRequests.Count > 0)
            {
                Logger.AllLog(Identity, $"Add start urls to scheduler, count {Site.StartRequests.Count}.", LogLevel.Info);
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
                Logger.AllLog(Identity, "Add start urls to scheduler, count 0.", LogLevel.Info);
            }
        }

        private void InjectCookie()
        {
            CookieInjector?.Inject(this, false);
        }

        private void InitFileCloseSignals()
        {
            _closeSignalFiles = new[] { Path.Combine(Env.BaseDirectory, $"{TaskId}_close"), Path.Combine(Env.BaseDirectory, $"{Identity}_close") };
            foreach (var closeSignal in _closeSignalFiles)
            {
                if (File.Exists(closeSignal))
                {
                    File.Delete(closeSignal);
                }
            }
        }

        private void ReportStatus() => Monitor?.Report(Stat.ToString(),
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
                Monitor = string.IsNullOrEmpty(Env.HubServiceUrl) ? new NLogMonitor(TaskId, Identity) : new HttpMonitor(TaskId, Identity);
            }
        }

        private int CalculateMonitorInterval()
        {

            if (Scheduler.LeftRequestsCount > 10)
            {
                return 10;
            }   
            else if (Scheduler.LeftRequestsCount > 5)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        private void PrepareErrorRequestsLogFile()
        {
            _errorRequestFile = BasePipeline.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));

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
                    _errorRequestFile = BasePipeline.PrepareFile(Path.Combine(Env.BaseDirectory, "ErrorRequests", Identity, $"errors.{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt"));
                }
                Thread.Sleep(500);
            }
        }

        private void InitSiteProperty()
        {
             
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
    }
}