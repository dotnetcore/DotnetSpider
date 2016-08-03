using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Log;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Linq;
using Java2Dotnet.Spider.Ioc;
using Java2Dotnet.Spider.Core.Monitor;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class SpiderMonitor
	{
		private static SpiderMonitor _instanse;
		private static readonly object Locker = new object();
		private readonly Dictionary<ISpider, Timer> _data = new Dictionary<ISpider, Timer>();

		private SpiderMonitor()
		{
		}

		public SpiderMonitor Register(params Spider[] spiders)
		{
			lock (this)
			{
				foreach (Spider spider in spiders)
				{
					if (!_data.ContainsKey(spider))
					{
						Timer timer = new Timer(new TimerCallback(SaveStatus), spider, 0, 2000);
						_data.Add(spider, timer);
					}
				}
				return this;
			}
		}

		private void SaveStatus(object obj)
		{
			foreach (var service in ServiceProvider.Get<IMonitorService>())
			{
				if (service.IsValid)
				{
					service.SaveStatus(obj);
				}
			}
		}

		public void Dispose()
		{
			foreach (var service in ServiceProvider.Get<IMonitorService>())
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