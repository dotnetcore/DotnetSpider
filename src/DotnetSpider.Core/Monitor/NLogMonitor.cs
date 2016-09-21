using DotnetSpider.Core.Common;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitor
	{
		private readonly NLog.ILogger _logger = LogManager.GetLogger("NLogMonitor");

		public bool IsEnabled => true;

		public NLogMonitor()
		{
			NLogUtils.Init(true);
		}

		public void Report(SpiderStatus status)
		{
			string msg = $"Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Thread {status.ThreadNum}";
			LogEventInfo theEvent = new LogEventInfo(NLog.LogLevel.Trace, "", msg);
			theEvent.Properties["UserId"] = status.UserId;
			theEvent.Properties["TaskGroup"] = status.TaskGroup;
			theEvent.Properties["Identity"] = status.Identity;
			theEvent.Properties["Status"] = status.Status;
			theEvent.Properties["Message"] = msg;

			if (SpiderConsts.SaveLogAndStatusToDb)
			{
				NetworkCenter.Current.Execute("nm", () =>
				{
					_logger.Log(theEvent);
				});
			}
			else
			{
				_logger.Log(theEvent);
			}
		}

		public void Dispose()
		{
		}
	}
}
