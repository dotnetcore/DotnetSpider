﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Proxy;
using DotnetSpider.Core.Scheduler;
using Newtonsoft.Json;
using NLog;

namespace DotnetSpider.Core
{
	/// <summary>
	/// A spider contains four modules: Downloader, Scheduler, PageProcessor and Pipeline. 
	/// </summary>
	public class Spider : ISpider
	{
		protected ILogger Logger { get; set; }
		protected DateTime StartTime { get; private set; }
		protected DateTime FinishedTime { get; private set; } = DateTime.MinValue;
		protected bool IsExitWhenComplete { get; set; } = true;
		protected IPageProcessor PageProcessor { get; set; }
		protected List<IPipeline> Pipelines { get; set; } = new List<IPipeline>();
		protected bool IsExited { get; set; }
		protected int WaitInterval = 10;
		protected Status Stat = Status.Init;

		#region ITask

		public string Identity { get; set; }
		public string UserId { get; set; }
		public string TaskGroup { get; set; }

		#endregion

		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public bool SpawnUrl { get; set; } = true;
		public bool SkipWhenResultIsEmpty { get; set; } = false;
		public bool RetryWhenResultIsEmpty { get; set; } = false;
		public Site Site { get; protected set; }
		public IDownloader Downloader { get; set; }
		public Status StatusCode => Stat;
		public event SpiderEvent OnSuccess;
		public event SpiderClosingHandler SpiderClosing;
		public Dictionary<string, dynamic> Settings { get; } = new Dictionary<string, dynamic>();
		public int EmptySleepTime { get; set; } = 15000;

		private int _waitCountLimit = 1500;
		private bool _init;
		private IScheduler _scheduler;
		private static bool _printedInfo;
		private FileInfo _errorRequestFile;
		private readonly Random _random = new Random();

		/// <summary>
		/// Create a spider with pageProcessor.
		/// </summary>
		/// <param name="site"></param>
		/// <param name="pageProcessor"></param>
		/// <returns></returns>
		public static Spider Create(Site site, IPageProcessor pageProcessor)
		{
			return new Spider(site, Guid.NewGuid().ToString(), null, null, pageProcessor, new QueueDuplicateRemovedScheduler());
		}

		/// <summary>
		/// Create a spider with pageProcessor and scheduler
		/// </summary>
		/// <param name="site"></param>
		/// <param name="pageProcessor"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static Spider Create(Site site, IPageProcessor pageProcessor, IScheduler scheduler)
		{
			return new Spider(site, Guid.NewGuid().ToString(), null, null, pageProcessor, scheduler);
		}

		/// <summary>
		/// Create a spider with indentify, pageProcessor, scheduler.
		/// </summary>
		/// <param name="site"></param>
		/// <param name="identify"></param>
		/// <param name="taskGroup"></param>
		/// <param name="pageProcessor"></param>
		/// <param name="scheduler"></param>
		/// <param name="userid"></param>
		/// <returns></returns>
		public static Spider Create(Site site, string identify, string userid, string taskGroup, IPageProcessor pageProcessor, IScheduler scheduler)
		{
			return new Spider(site, identify, userid, taskGroup, pageProcessor, scheduler);
		}

		protected Spider()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			LogManagerHelper.InitLogManager();
			Logger = LogManager.GetCurrentClassLogger();
			IsExited = false;
		}

		/// <summary>
		/// Create a spider with pageProcessor.
		/// </summary>
		/// <param name="site"></param>
		/// <param name="identity"></param>
		/// <param name="taskGroup"></param>
		/// <param name="pageProcessor"></param>
		/// <param name="scheduler"></param>
		/// <param name="userid"></param>
		protected Spider(Site site, string identity, string userid, string taskGroup, IPageProcessor pageProcessor, IScheduler scheduler) : this()
		{
			Identity = identity;
			UserId = userid;
			PageProcessor = pageProcessor;
			Site = site;
			TaskGroup = taskGroup;
			Scheduler = scheduler;

			CheckIfSettingsCorrect();
		}

		protected void CheckIfSettingsCorrect()
		{
			if (string.IsNullOrWhiteSpace(Identity) || string.IsNullOrEmpty(Identity))
			{
				Identity = string.IsNullOrEmpty(Site.Domain) ? Guid.NewGuid().ToString() : Site.Domain;
			}

			if (string.IsNullOrEmpty(UserId) || string.IsNullOrWhiteSpace(UserId))
			{
				UserId = "DotnetSpider";
			}

			if (string.IsNullOrEmpty(TaskGroup) || string.IsNullOrWhiteSpace(TaskGroup))
			{
				TaskGroup = "Default";
			}

			if (Identity.Length > 100)
			{
				throw new SpiderException("Length of Identity should less than 100.");
			}

			if (UserId.Length > 100)
			{
				throw new SpiderException("Length of UserId should less than 100.");
			}

			if (TaskGroup.Length > 100)
			{
				throw new SpiderException("Length of TaskGroup should less than 100.");
			}

			if (PageProcessor == null)
			{
				throw new SpiderException("PageProcessor should not be null.");
			}

			if (Site == null)
			{
				Site = new Site();
			}
			PageProcessor.Site = Site;
			Scheduler = Scheduler ?? new QueueDuplicateRemovedScheduler();
			Downloader = Downloader ?? new HttpClientDownloader();
		}

		public IScheduler Scheduler
		{
			get
			{
				return _scheduler;
			}
			set
			{
				CheckIfRunning();
				_scheduler = value;
			}
		}

		/// <summary>
		/// Start with more than one threads
		/// </summary>
		/// <param name="threadNum"></param>
		/// <returns></returns>
		public virtual Spider SetThreadNum(int threadNum)
		{
			CheckIfRunning();

			if (threadNum <= 0)
			{
				throw new ArgumentException("threadNum should be more than one!");
			}

			ThreadNum = threadNum;

			return this;
		}

		public void SetSite(Site site)
		{
			CheckIfRunning();

			Site = site;
		}

		public bool IfExited()
		{
			return IsExited;
		}

		public void SetIdentity(string identity)
		{
			CheckIfRunning();

			Identity = identity;
		}

		/// <summary>
		/// Set wait time when no url is polled.
		/// </summary>
		/// <param name="emptySleepTime"></param>
		public void SetEmptySleepTime(int emptySleepTime)
		{
			CheckIfRunning();

			if (emptySleepTime >= 1000)
			{
				EmptySleepTime = emptySleepTime;
				_waitCountLimit = EmptySleepTime / WaitInterval;
			}
			else
			{
				throw new SpiderException("Sleep time should be large than 1000.");
			}
		}

		public void SetScheduler(IScheduler scheduler)
		{
			CheckIfRunning();
			Scheduler = scheduler;
		}

		public void SetTaskGroup(string taskGroup)
		{
			CheckIfRunning();
			TaskGroup = taskGroup;
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
				AddStartRequest(new Request(url, 1, null));
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
			AddStartRequest(new Request(url, 1, extras));
			return this;
		}

		public Spider AddStartUrl(ICollection<string> urls)
		{
			foreach (string url in urls)
			{
				AddStartRequest(new Request(url, 1, null));
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
			CheckIfRunning();
			Pipelines.Add(pipeline);
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
			return Pipelines;
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
		/// Set the downloader of spider
		/// </summary>
		/// <param name="downloader"></param>
		/// <returns></returns>
		public Spider SetDownloader(IDownloader downloader)
		{
			CheckIfRunning();
			Downloader = downloader;
			return this;
		}

		public void InitComponent()
		{
			if (_init)
			{
				return;
			}

			if (Pipelines == null || Pipelines.Count == 0)
			{
				throw new SpiderException("Pipelines should not be null.");
			}

			Scheduler.Init(this);
#if !NET_CORE
			_errorRequestFile = BasePipeline.PrepareFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));
#else
			_errorRequestFile = BasePipeline.PrepareFile(Path.Combine(AppContext.BaseDirectory, "ErrorRequests", Identity, "errors.txt"));
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			Console.CancelKeyPress += ConsoleCancelKeyPress;

			foreach (var pipeline in Pipelines)
			{
				pipeline.InitPipeline(this);
			}

			if (Site.StartRequests != null && Site.StartRequests.Count > 0)
			{
				Logger.SaveLog(LogInfo.Create($"[步骤 1] 添加链接到调度中心, 数量: {Site.StartRequests.Count}.", Logger.Name, this, LogLevel.Info));
				if ((Scheduler is QueueDuplicateRemovedScheduler) || (Scheduler is PriorityScheduler))
				{
					Parallel.ForEach(Site.StartRequests, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, request =>
					{
						Scheduler.Push(request);
					});
				}
				else
				{
					Scheduler.Load(new HashSet<Request>(Site.StartRequests));
					ClearStartRequests();
				}
			}
			else
			{
				Logger.SaveLog(LogInfo.Create("[步骤 1] 添加链接到调度中心, 数量: 0.", Logger.Name, this, LogLevel.Info));
			}

			_waitCountLimit = EmptySleepTime / WaitInterval;

			if (Site.MinSleepTime > Site.MaxSleepTime)
			{
				Site.MaxSleepTime = Site.MinSleepTime;
			}

			_init = true;
		}

		public virtual void Run(params string[] arguments)
		{
			CheckIfRunning();

			CheckIfSettingsCorrect();

			Stat = Status.Running;
			IsExited = false;

#if !NET_CORE
			// 开启多线程支持
			ServicePointManager.DefaultConnectionLimit = 1000;
#endif

			InitComponent();

			if (StartTime == DateTime.MinValue)
			{
				StartTime = DateTime.Now;
			}

			Parallel.For(0, ThreadNum, new ParallelOptions
			{
				MaxDegreeOfParallelism = ThreadNum
			}, i =>
			{
				int waitCount = 0;
				bool firstTask = false;

				var downloader = Downloader.Clone();

				while (Stat == Status.Running)
				{
					Request request = Scheduler.Poll();

					if (request == null)
					{
						if (waitCount > _waitCountLimit && IsExitWhenComplete)
						{
							Stat = Status.Finished;
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
							ProcessRequest(request, downloader);
							Thread.Sleep(_random.Next(Site.MinSleepTime, Site.MaxSleepTime));
#if TEST
							System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
							sw.Reset();
							sw.Start();
#endif

							_OnSuccess(request);
#if TEST
							sw.Stop();
							Console.WriteLine("OnSuccess:" + (sw.ElapsedMilliseconds).ToString());
#endif
						}
						catch (Exception e)
						{
							OnError(request);
							Logger.SaveLog(LogInfo.Create($"采集失败: {request.Url}.", Logger.Name, this, LogLevel.Error, e));
						}
						finally
						{
							if (request.GetExtra(Request.Proxy) != null)
							{
								var statusCode = request.GetExtra(Request.StatusCode);
								Site.ReturnHttpProxy(request.GetExtra(Request.Proxy) as UseSpecifiedUriWebProxy, statusCode == null ? HttpStatusCode.Found : (HttpStatusCode)statusCode);
							}
						}

						if (!firstTask)
						{
							Thread.Sleep(3000);
							firstTask = true;
						}
					}
				}
			});

			FinishedTime = DateTime.Now;

			foreach (IPipeline pipeline in Pipelines)
			{
				SafeDestroy(pipeline);
			}

			SpiderClosing?.Invoke();

			if (Stat == Status.Finished)
			{
				OnClose();
				Logger.SaveLog(LogInfo.Create($"采集结束, 运行时间: {(FinishedTime - StartTime).TotalSeconds} 秒.", Logger.Name, this, LogLevel.Info));
			}

			if (Stat == Status.Stopped)
			{
				Logger.SaveLog(LogInfo.Create($"采集暂停, 运行时间: {(FinishedTime - StartTime).TotalSeconds} 秒.", Logger.Name, this, LogLevel.Info));
			}

			if (Stat == Status.Exited)
			{
				Logger.SaveLog(LogInfo.Create($"采集退出, 运行时间: {(FinishedTime - StartTime).TotalSeconds} 秒.", Logger.Name, this, LogLevel.Info));
			}

			IsExited = true;
		}

		public static void PrintInfo()
		{
			if (!_printedInfo)
			{
				Console.WriteLine("=============================================================");
				Console.WriteLine("== DotnetSpider is an open source .Net spider              ==");
				Console.WriteLine("== It's a light, stable, high performce spider             ==");
				Console.WriteLine("== Support multi thread, ajax page, http                   ==");
				Console.WriteLine("== Support save data to file, mysql, mssql, mongodb etc    ==");
				Console.WriteLine("== License: LGPL3.0                                        ==");
				Console.WriteLine("== Version: 0.9.10                                         ==");
				Console.WriteLine("== Author: zlzforever@163.com                              ==");
				Console.WriteLine("=============================================================");
				_printedInfo = true;
			}
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			}).ContinueWith(t =>
			{
				if (t.Exception != null)
				{
					Logger.Error(t.Exception.Message);
				}
			});
		}

		public void Stop()
		{
			Stat = Status.Stopped;
			Logger.SaveLog(LogInfo.Create("停止任务中...", Logger.Name, this, LogLevel.Warn));
		}

		public void Exit()
		{
			Stat = Status.Exited;
			Logger.SaveLog(LogInfo.Create("退出任务中...", Logger.Name, this, LogLevel.Warn));
			SpiderClosing?.Invoke();
		}

		protected void OnClose()
		{
			foreach (var pipeline in Pipelines)
			{
				SafeDestroy(pipeline);
			}

			SafeDestroy(Scheduler);
			SafeDestroy(PageProcessor);
			SafeDestroy(Downloader);
		}

		protected void OnError(Request request)
		{
			lock (this)
			{
				File.AppendAllText(_errorRequestFile.FullName, JsonConvert.SerializeObject(request) + Environment.NewLine, Encoding.UTF8);
			}
			Scheduler.IncreaseErrorCounter();
		}

		protected void _OnSuccess(Request request)
		{
			Scheduler.IncreaseSuccessCounter();
			OnSuccess?.Invoke(request);
		}

		protected Page AddToCycleRetry(Request request, Site site, bool resultIsEmpty = false)
		{
			Page page = new Page(request, site.ContentType);
			if (!resultIsEmpty)
			{
				dynamic cycleTriedTimesObject = request.GetExtra(Request.CycleTriedTimes);
				if (cycleTriedTimesObject == null)
				{
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.CycleTriedTimes, 1));
				}
				else
				{
					int cycleTriedTimes = (int)cycleTriedTimesObject;
					cycleTriedTimes++;
					if (cycleTriedTimes >= site.CycleRetryTimes)
					{
						return null;
					}
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.CycleTriedTimes, cycleTriedTimes));
				}
				page.IsNeedCycleRetry = true;
				return page;
			}
			else
			{
				dynamic cycleTriedTimesObject = request.GetExtra(Request.ResultIsEmptyTriedTimes);
				if (cycleTriedTimesObject == null)
				{
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.ResultIsEmptyTriedTimes, 1));
				}
				else
				{
					int cycleTriedTimes = (int)cycleTriedTimesObject;
					cycleTriedTimes++;
					if (cycleTriedTimes >= site.CycleRetryTimes)
					{
						return null;
					}
					request.Priority = 0;
					page.AddTargetRequest(request.PutExtra(Request.ResultIsEmptyTriedTimes, cycleTriedTimes));
				}
				page.IsNeedCycleRetry = true;
				return page;
			}
		}

		protected void ProcessRequest(Request request, IDownloader downloader)
		{
			Page page = null;
#if TEST
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
#endif

			try
			{
#if TEST
				sw.Reset();
				sw.Start();
#endif
				page = downloader.Download(request, this);

#if TEST
				sw.Stop();
				Console.WriteLine("Download:" + (sw.ElapsedMilliseconds).ToString());
#endif
				if (page.IsSkip)
				{
					return;
				}

#if TEST
				sw.Reset();
				sw.Start();
#endif
				PageProcessor.Process(page);
#if TEST
				sw.Stop();
				Console.WriteLine("Process:" + (sw.ElapsedMilliseconds).ToString());
#endif
			}
			//catch (Redial.RedialException re)
			//{
			//	if (Site.CycleRetryTimes > 0)
			//	{
			//		page = AddToCycleRetry(request, Site);
			//	}
			//	Logger.Warn(re.Message);
			//}
			catch (DownloadException de)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				Logger.SaveLog(LogInfo.Create($"下载{request.Url}失败:{de.Message}", Logger.Name, this, LogLevel.Warn));
			}
			catch (HttpRequestException he)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				Logger.SaveLog(LogInfo.Create($"下载{request.Url}失败:{he.Message}", Logger.Name, this, LogLevel.Warn));
			}
			catch (Exception e)
			{
				if (Site.CycleRetryTimes > 0)
				{
					page = AddToCycleRetry(request, Site);
				}
				Logger.SaveLog(LogInfo.Create($"解析数据失败: {request.Url}, 请检查您的数据抽取设置: {e.Message}", Logger.Name, this, LogLevel.Warn));
			}

			//watch.Stop();
			//Logger.Info("dowloader cost time:" + watch.ElapsedMilliseconds);

			if (page == null)
			{
				OnError(request);
				return;
			}

			if (page.IsNeedCycleRetry)
			{
				ExtractAndAddRequests(page, true);
				return;
			}

			//watch.Stop();
			//Logger.Info("process cost time:" + watch.ElapsedMilliseconds);

			if (!page.MissTargetUrls)
			{
				if (!(SkipWhenResultIsEmpty && page.ResultItems.IsSkip))
				{
					ExtractAndAddRequests(page, SpawnUrl);
				}
			}
#if TEST
			sw.Reset();
			sw.Start();
#endif
			if (!page.ResultItems.IsSkip)
			{
				foreach (IPipeline pipeline in Pipelines)
				{
					pipeline.Process(page.ResultItems);
				}
				Logger.SaveLog(LogInfo.Create($"采集: {request.Url} 成功.", Logger.Name, this, LogLevel.Info));
			}
			else
			{
				if (RetryWhenResultIsEmpty)
				{
					if (Site.CycleRetryTimes > 0)
					{
						page = AddToCycleRetry(request, Site, true);
						if (page != null && page.IsNeedCycleRetry)
						{
							ExtractAndAddRequests(page, true);
						}
						Logger.SaveLog(LogInfo.Create($"解析: {request.Url} 结果为 0, 重新尝试采集.", Logger.Name, this, LogLevel.Info));
					}
					else
					{
						Logger.SaveLog(LogInfo.Create($"采集: {request.Url} 成功, 解析结果为 0.", Logger.Name, this, LogLevel.Info));
					}
				}
				else
				{
					Logger.SaveLog(LogInfo.Create($"采集: {request.Url} 成功, 解析结果为 0.", Logger.Name, this, LogLevel.Info));
				}
			}

#if TEST
			sw.Stop();
			Console.WriteLine("IPipeline:" + (sw.ElapsedMilliseconds).ToString());
#endif
		}

		protected void ExtractAndAddRequests(Page page, bool spawnUrl)
		{
			if (spawnUrl && page.Request.NextDepth < Deep && page.TargetRequests != null && page.TargetRequests.Count > 0)
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
				throw new SpiderException("Spider is already running!");
			}
		}

		private void ClearStartRequests()
		{
			lock (this)
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
			var disposable = obj as IDisposable;
			if (disposable != null)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception e)
				{
					Logger.Warn(e.ToString());
				}
			}
		}

		private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Stop();
			while (!IsExited)
			{
				Thread.Sleep(1500);
			}
		}

		public IMonitorable GetMonitor()
		{
			return Scheduler;
		}

		public void Dispose()
		{
			CheckIfRunning();

			int i = 0;
			while (!IsExited)
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
	}
}