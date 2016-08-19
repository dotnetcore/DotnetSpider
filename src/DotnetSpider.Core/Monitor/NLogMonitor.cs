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
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Warn, "", msg);
			theEvent.Properties["UserId"] = status.UserId;
			theEvent.Properties["TaskGroup"] = status.TaskGroup;
			theEvent.Properties["Identity"] = status.Identity;
			theEvent.Properties["Status"] = status.Status;
			theEvent.Properties["Message"] = msg;
			NetworkCenter.Current.Execute("nm", () =>
			{
				_logger.Log(theEvent);
			});
		}

		public void Dispose()
		{
		}
	}
}
