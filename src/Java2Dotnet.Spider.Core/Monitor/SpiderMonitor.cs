using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Linq;
using Java2Dotnet.Spider.Core.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class SpiderMonitor
	{
		private static SpiderMonitor _instanse;
		private static readonly object Locker = new object();
		private readonly Dictionary<ISpider, Timer> _data = new Dictionary<ISpider, Timer>();
		private List<IMonitorService> monitorServices;

		private SpiderMonitor()
		{
			monitorServices = IocExtension.ServiceProvider.GetServices<IMonitorService>().ToList();
		}

		public SpiderMonitor Register(params Spider[] spiders)
		{
			lock (this)
			{
				foreach (Spider spider in spiders)
				{
					if (!_data.ContainsKey(spider))
					{
						Timer timer = new Timer(new TimerCallback(WatchStatus), spider, 0, 2000);
						_data.Add(spider, timer);
					}
				}
				return this;
			}
		}

		private void WatchStatus(object obj)
		{
			foreach (var service in monitorServices)
			{
				if (service.IsEnabled)
				{
					var spider = obj as Spider;
					var monitor = spider.Scheduler as IMonitorableScheduler;
					service.Watch(new SpiderStatus
					{
						Code = spider.StatusCode.ToString(),
						Error = monitor.GetErrorRequestsCount(),
						Identity = spider.Identity,
						Left = monitor.GetLeftRequestsCount(),
						Machine = SystemInfo.HostName,
						Success = monitor.GetSuccessRequestsCount(),
						TaskGroup = spider.TaskGroup,
						ThreadNum = spider.ThreadNum,
						Total = monitor.GetTotalRequestsCount(),
						UserId = spider.UserId,
						Timestamp = DateTime.Now.ToString()
					});
				}
			}
		}

		public void Dispose()
		{
			foreach (var service in monitorServices)
			{
				service.Dispose();
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
	}
}