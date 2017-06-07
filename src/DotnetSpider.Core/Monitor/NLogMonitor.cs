using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitor
	{
		private readonly NLog.ILogger _logger = LogManager.GetLogger("NLogMonitor");

		public bool IsEnabled => true;

		public NLogMonitor()
		{
			NLogExtensions.Init();
		}

		public void Report(SpiderStatus status)
		{
			string msg = $"Left {status.Left} Success {status.Success} Error {status.Error} Total {status.Total} Dowload {status.AvgDownloadSpeed} Extract {status.AvgProcessorSpeed} Pipeline {status.AvgPipelineSpeed}";
			LogEventInfo theEvent = new LogEventInfo(NLog.LogLevel.Trace, "", msg);
			theEvent.Properties["Identity"] = status.Identity;
			theEvent.Properties["ThreadNum"] = status.ThreadNum;
			theEvent.Properties["Status"] = status.Status;
			theEvent.Properties["Left"] = status.Left;
			theEvent.Properties["Success"] = status.Success;
			theEvent.Properties["Error"] = status.Error;
			theEvent.Properties["Total"] = status.Total;
			theEvent.Properties["AvgDownloadSpeed"] = status.AvgDownloadSpeed;
			theEvent.Properties["AvgProcessorSpeed"] = status.AvgProcessorSpeed;
			theEvent.Properties["AvgPipelineSpeed"] = status.AvgPipelineSpeed;
			
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
