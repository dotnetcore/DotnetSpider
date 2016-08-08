using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Scheduler;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Core.Monitor
{
	public class SpiderMonitor
	{
		private static SpiderMonitor _instanse;
		private static readonly object Locker = new object();
		private readonly Dictionary<ISpider, Timer> _data = new Dictionary<ISpider, Timer>();
		private readonly List<IMonitorService> _monitorServices;

		private SpiderMonitor()
		{
			_monitorServices = IocExtension.ServiceProvider.GetServices<IMonitorService>().ToList();
		}

		public SpiderMonitor Register(params Spider[] spiders)
		{
			lock (this)
			{
				foreach (Spider spider in spiders)
				{
					if (!_data.ContainsKey(spider))
					{
						Timer timer = new Timer(WatchStatus, spider, 0, 2000);
						_data.Add(spider, timer);
					}
				}
				return this;
			}
		}

		private void WatchStatus(object obj)
		{
			foreach (var service in _monitorServices)
			{
				if (service.IsEnabled)
				{
					var spider = (Spider)obj;
					var monitor = (IMonitorableScheduler)spider.Scheduler ;
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
						Timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture)
					});
				}
			}
		}

		public void Dispose()
		{
			foreach (var service in _monitorServices)
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