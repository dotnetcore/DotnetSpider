using System;
using System.Net;
using NLog;

namespace DotnetSpider.Core
{
	public class LogInfo
	{
		public static string Machine = Dns.GetHostName();

		public static LogEventInfo Create(string message, string loggerName, ITask task, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, loggerName, message) { Exception = e };
			theEvent.Properties["UserId"] = task == null ? "" : task.UserId;
			theEvent.Properties["TaskGroup"] = task == null ? "" : task.TaskGroup;
			return theEvent;
		}
	}
}
