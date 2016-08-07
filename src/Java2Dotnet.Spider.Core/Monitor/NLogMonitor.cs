using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Scheduler;
using System.IO;
using NLog;

namespace Java2Dotnet.Spider.Core.Monitor
{
	public class NLogMonitor : IMonitorService
	{
		public bool IsEnabled
		{
			get
			{
				return true;
			}
		}

		private ILogger Logger = LogManager.GetCurrentClassLogger();
 
		public void Watch(SpiderStatus status)
		{
			string msg = $"[{status.UserId}][{status.TaskGroup}][{status.Identity}]{Environment.NewLine} Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Thread {status.ThreadNum}";
			Logger.Warn(msg);
		}

		public void Dispose()
		{
		}
	}
}
