using System;
using System.IO;
using System.Net;
using DotnetSpider.Core.Common;
using NLog;
using NLog.Config;

namespace DotnetSpider.Core
{
	public class LogManagerHelper
	{
		private static bool _init;

		public static void InitLogManager()
		{
			if (!_init)
			{
				string nlogConfigPath = Path.Combine(SpiderEnviroment.BaseDirectory, "nlog.config");
				if (File.Exists(nlogConfigPath))
				{
					File.AppendAllText(nlogConfigPath, Resource.nlog);
					LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
				}
				_init = true;
			}
		}
	}

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
