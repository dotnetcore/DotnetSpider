using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();
		protected readonly string TaskId;
		protected readonly string Identity;

		public NLogMonitor(string taskId, string identity)
		{
			TaskId = taskId;
			Identity = identity;
		}

		public virtual void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			string msg = $"Left {left} Success {success} Error {error} Total {total} Dowload {avgDownloadSpeed} Extract {avgProcessorSpeed} Pipeline {avgPipelineSpeed}";
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Trace, "", msg);
			theEvent.Properties["Identity"] = Identity;
			theEvent.Properties["NodeId"] = NodeId.Id;
			Logger.Log(theEvent);
		}
	}
}
