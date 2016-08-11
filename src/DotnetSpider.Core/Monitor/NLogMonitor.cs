using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitorService
	{
		public bool IsEnabled => true;

		private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
 
		public void Watch(SpiderStatus status)
		{
			string msg = $"Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Thread {status.ThreadNum}";
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, "", msg);
			theEvent.Properties["UserId"] = status.UserId;
			theEvent.Properties["TaskGroup"] = status.TaskGroup;
			theEvent.Properties["Identity"] = status.Identity;
			theEvent.Properties["Status"] = status.Status;
			_logger.Log(theEvent);

			//string msg = $"[{status.UserId}][{status.TaskGroup}][{status.Identity}]{Environment.NewLine} Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Thread {status.ThreadNum}";
			//_Logger.Log(msg);
		}

		public void Dispose()
		{
		}
	}
}
