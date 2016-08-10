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
			LogEventInfo theEvent = new LogEventInfo(level, loggerName, message);
			theEvent.Exception = e;
			theEvent.Properties["UserId"] = task.UserId;
			theEvent.Properties["TaskGroup"] = task.TaskGroup;
			return theEvent;
			//return $"[{task.UserId}][{task.TaskGroup}] {message}";
		}
	}
}
