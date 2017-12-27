using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// NLog 状态监控, 依据NLog配置上报状态到控制台或者日志文件中
	/// </summary>
	public class NLogMonitor : IMonitor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		public virtual void Report(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			string msg = $"Left {left} Success {success} Error {error} Total {total} Dowload {avgDownloadSpeed} Extract {avgProcessorSpeed} Pipeline {avgPipelineSpeed}";
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Trace, "", msg);
			theEvent.Properties["Identity"] = identity;
			theEvent.Properties["NodeId"] = NodeId.Id;
			Logger.Log(theEvent);
		}
	}
}
