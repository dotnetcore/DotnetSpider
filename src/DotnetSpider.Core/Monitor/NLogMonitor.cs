using System;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitorService
	{
		public bool IsEnabled => true;

		private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
 
		public void Watch(SpiderStatus status)
		{
			string msg = $"[{status.UserId}][{status.TaskGroup}][{status.Identity}]{Environment.NewLine} Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Thread {status.ThreadNum}";
			_logger.Warn(msg);
		}

		public void Dispose()
		{
		}
	}
}
