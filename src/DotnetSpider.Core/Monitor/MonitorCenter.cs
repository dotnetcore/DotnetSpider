using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DotnetSpider.Core.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Monitor
{
	public static class MonitorCenter
	{
		private static readonly object Locker = new object();
		private static readonly Dictionary<ISpider, Task> MoniteSpiders = new Dictionary<ISpider, Task>();
		private static readonly List<IMonitor> Monitors;

		static MonitorCenter()
		{
			Monitors = IocContainer.Default.GetServices<IMonitor>().ToList();
			if (Monitors.Count == 0)
			{
				Monitors = new List<IMonitor> { new NLogMonitor() };
			}
		}

		public static void Register(params Spider[] spiders)
		{
			lock (Locker)
			{
				foreach (Spider sp in spiders)
				{
					if (!MoniteSpiders.ContainsKey(sp))
					{
						MoniteSpiders.Add(sp, Task.Factory.StartNew(s =>
						{
							var spider = (Spider)s;
							while (!spider.IfExited())
							{
								Report(spider);

								Thread.Sleep(2000);
							}

							Report(spider);
						}, sp));
					}
				}
			}
		}

		private static void Report(Spider spider)
		{
			foreach (var service in Monitors)
			{
				if (service.IsEnabled)
				{
					var monitor = spider.GetMonitor();

					service.Report(new SpiderStatus
					{
						Status = spider.StatusCode.ToString(),
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

		public static void Dispose()
		{
			while (MoniteSpiders.Any(p => !p.Value.IsCompleted))
			{
				Thread.Sleep(500);
			}

			foreach (var service in Monitors)
			{
				service.Dispose();
			}
		}
	}
}