using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Extension.Scheduler;
using Java2Dotnet.Spider.JLog;
using Newtonsoft.Json;
using RedisSharp;
using System.Net.Http;
using System.Text;
using System.Net;

namespace Java2Dotnet.Spider.Extension.Monitor
{
	public class SpiderMonitor
	{
		private static SpiderMonitor _instanse;
		private static readonly object Locker = new object();
		private readonly Dictionary<ISpider, MonitorSpiderListener> _data = new Dictionary<ISpider, MonitorSpiderListener>();

		private SpiderMonitor()
		{
		}

		public SpiderMonitor Register(params Core.Spider[] spiders)
		{
			lock (this)
			{
				foreach (Core.Spider spider in spiders)
				{
					if (!_data.ContainsKey(spider))
					{
						MonitorSpiderListener monitorSpiderListener = new MonitorSpiderListener(spider);
						_data.Add(spider, monitorSpiderListener);
					}
				}
				return this;
			}
		}

		public static SpiderMonitor Default
		{
			get
			{
				lock (Locker)
				{
					return _instanse ?? (_instanse = new SpiderMonitor());
				}
			}
		}

		public class MonitorSpiderListener : ISpiderStatus
		{
			protected static readonly ILog Logger = LogManager.GetLogger();
			private readonly AutomicLong _successCount = new AutomicLong(0);
			private readonly AutomicLong _errorCount = new AutomicLong(0);
			private readonly List<string> _errorUrls = new List<string>();
			private readonly Core.Spider _spider;
			private static SynchronizedList<Task<HttpResponseMessage>> StatusUpLoadTasks = new SynchronizedList<Task<HttpResponseMessage>>();
			private static string StatusServer;
			private static HttpClient _client = new HttpClient();

			static MonitorSpiderListener()
			{
				StatusServer = ConfigurationManager.Get("statusHost");
			}

			public MonitorSpiderListener(Core.Spider spider)
			{
				_spider = spider;

				if (spider.SaveStatus && !string.IsNullOrEmpty(StatusServer))
				{
					spider.RequestedFailEvent += OnError;
					spider.RequestedSuccessEvent += OnSuccess;
					spider.SpiderClosingEvent += OnClose;

					Task.Factory.StartNew(() =>
					{
						while (true)
						{
							try
							{
								PostStatus();
							}
							catch (Exception)
							{
								// ignored
							}

							Thread.Sleep(5000);
						}
					});
				}
			}

			public void PostStatus()
			{
				var status = new
				{
					Message = new
					{
						ErrorPageCount,
						LeftPageCount,
						Speed,
						StartTime,
						EndTime,
						Status,
						SuccessPageCount,
						ThreadCount,
						TotalPageCount
					},
					Name,
					Machine = Log.Machine,
					UserId = Log.UserId,
					TaskId = Log.TaskId
				};

				var task = _client.PostAsync(StatusServer, new StringContent(JsonConvert.SerializeObject(status), Encoding.UTF8, "application/json"));
				StatusUpLoadTasks.Add(task);
				task.ContinueWith((t) =>
				{
					StatusUpLoadTasks.Remove(t);
				});
			}

			private  void WaitForExit()
			{
				while (true)
				{
					if (StatusUpLoadTasks.Count() == 0)
					{
						break;
					}
					Thread.Sleep(100);
				}
			}

			public void OnSuccess(Request request)
			{
				_successCount.Inc();
			}

			public void OnError(Request request)
			{
				_errorUrls.Add(request.Url.ToString());
				_errorCount.Inc();
			}

			public void OnClose()
			{
				PostStatus();

				WaitForExit();
			}

			public long SuccessPageCount => _successCount.Value;

			public long ErrorPageCount => _errorCount.Value;

			public List<string> ErrorPages => _errorUrls;

			public string Name => _spider.Identity;

			public long LeftPageCount
			{
				get
				{
					IMonitorableScheduler scheduler = _spider.Scheduler as IMonitorableScheduler;
					if (scheduler != null)
					{
						return scheduler.GetLeftRequestsCount(_spider);
					}
					Logger.Warn("Get leftPageCount fail, try to use a Scheduler implement MonitorableScheduler for monitor count!");
					return -1;
				}
			}

			public long TotalPageCount
			{
				get
				{
					IMonitorableScheduler scheduler = _spider.Scheduler as IMonitorableScheduler;
					if (scheduler != null)
					{
						return scheduler.GetTotalRequestsCount(_spider);
					}
					Logger.Warn("Get totalPageCount fail, try to use a Scheduler implement MonitorableScheduler for monitor count!");
					return -1;
				}
			}

			public string Status => _spider.StatusCode.ToString();

			public int ThreadCount => _spider.ThreadNum;

			public DateTime StartTime => _spider.StartTime;

			public DateTime EndTime => _spider.FinishedTime == DateTime.MinValue ? DateTime.Now : _spider.FinishedTime;

			public double Speed
			{
				get
				{
					double runSeconds = (DateTime.Now - StartTime).TotalSeconds;
					if (runSeconds > 0)
					{
						return SuccessPageCount / runSeconds;
					}
					return 0;
				}
			}
		}
	}
}