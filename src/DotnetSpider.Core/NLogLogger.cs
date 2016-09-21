using System;
using DotnetSpider.Core.Common;
using NLog;

namespace DotnetSpider.Core
{
	public class NLogLogger : ILogger
	{
		private readonly NLog.ILogger _logger;

		public NLogLogger()
		{
			NLogUtils.Init(true);
			_logger = LogManager.GetCurrentClassLogger();
		}

		public void Log(ITask spider, string message, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(GetNLogLevel(level), _logger.Name, message) { Exception = e };
			theEvent.Properties["UserId"] = spider == null ? "DotnetSpider" : spider.UserId;
			theEvent.Properties["TaskGroup"] = spider == null ? "Default" : spider.TaskGroup;
			theEvent.Properties["Identity"] = spider == null ? "Default" : spider.Identity;
			_logger.Log(theEvent);
		}

		private NLog.LogLevel GetNLogLevel(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
					{
						return NLog.LogLevel.Debug;
					}
				case LogLevel.Error:
					{
						return NLog.LogLevel.Error;
					}
				case LogLevel.Fatal:
					{
						return NLog.LogLevel.Fatal;
					}
				case LogLevel.Info:
					{
						return NLog.LogLevel.Info;
					}
				case LogLevel.Off:
					{
						return NLog.LogLevel.Off;
					}
				case LogLevel.Trace:
					{
						return NLog.LogLevel.Trace;
					}
				case LogLevel.Warn:
					{
						return NLog.LogLevel.Warn;
					}
				default:
					{
						return NLog.LogLevel.Info;
					}
			}
		}
	}
}
