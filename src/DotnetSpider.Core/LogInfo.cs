using System;
using System.Net;
using NLog;

namespace DotnetSpider.Core
{
	public class LogInfo
	{
		public static string Machine = Dns.GetHostName();

		public static LogEventInfo Create(string message, string loggerName, ISpider spider, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, loggerName, message) { Exception = e };
			theEvent.Properties["UserId"] = spider == null ? "" : spider.UserId;
			theEvent.Properties["TaskGroup"] = spider == null ? "" : spider.TaskGroup;
			theEvent.Properties["Identity"] = spider == null ? "" : spider.Identity;
			return theEvent;
		}
	}
}
