using System;
using NLog;

namespace DotnetSpider.Core.Infrastructure
{
	public class NLogLogger : ILogger
	{
		private readonly NLog.ILogger _logger;

		private static readonly Lazy<NLog.ILogger> Builder = new Lazy<NLog.ILogger>(() => LogManager.GetCurrentClassLogger());

		public NLogLogger()
		{
			NLogExtensions.Init();
			_logger = Builder.Value;
		}

		public void Log(IIdentity identity, string node, string message, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(GetNLogLevel(level), _logger.Name, message) { Exception = e };
			theEvent.Properties["Identity"] = identity == null ? "UNKNOW" : identity.Identity;
			theEvent.Properties["Node"] = node;
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
