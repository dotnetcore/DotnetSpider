using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Scheduler;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class ConsoleMonitor : IMonitorService
	{
		public bool IsValid
		{
			get
			{
				return true;
			}
		}

		public void Dispose()
		{
		}

		public void SaveStatus(dynamic spider)
		{
			spider.Logger.Warn($"Left: {spider.Scheduler.GetLeftRequestsCount()} Total: {spider.Scheduler.GetTotalRequestsCount()} Thread: {spider.ThreadNum}");
		}
	}
}
