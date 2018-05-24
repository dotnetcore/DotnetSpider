using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	public class NLogMonitor : IMonitor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();
        protected readonly string _taskId;
        protected readonly string _identity;
        public NLogMonitor(string taskId, string identity)
        {
            _taskId = taskId;
            _identity = identity;
        }


		public virtual void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			string msg = $"Left {left} Success {success} Error {error} Total {total} Dowload {avgDownloadSpeed} Extract {avgProcessorSpeed} Pipeline {avgPipelineSpeed}";
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Trace, "", msg);
			theEvent.Properties["Identity"] = _identity;
			theEvent.Properties["Node"] = NodeId.Id;
			Logger.Log(theEvent);
		}
	}
}
