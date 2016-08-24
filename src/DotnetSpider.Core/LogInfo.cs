using System;
using System.IO;
using System.Linq;
using System.Net;
using DotnetSpider.Core.Common;
using NLog;
using NLog.Config;
using NLog.Targets;

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
				if (!File.Exists(nlogConfigPath))
				{
					File.AppendAllText(nlogConfigPath, Resource.nlog);
				}
				XmlLoggingConfiguration configuration = new XmlLoggingConfiguration(nlogConfigPath);
				var connectString = Configuration.GetValue("logAndStatusConnectString");
				var logAndStatusTargets = configuration.AllTargets.Where(t => t.Name == "dblog" || t.Name == "dbstatus").ToList();
				if (!string.IsNullOrEmpty(connectString))
				{
					foreach (var logAndStatusTarget in logAndStatusTargets)
					{
						DatabaseTarget dbTarget = (DatabaseTarget)logAndStatusTarget;
						dbTarget.ConnectionString = connectString;
					}
				}

				var needDeleteRules = configuration.LoggingRules.Where(r => r.Targets.Any(t => t is DatabaseTarget && ((DatabaseTarget)t).ConnectionString == null)).ToList();
				foreach (var rule in needDeleteRules)
				{
					configuration.LoggingRules.Remove(rule);
				}

				configuration.Install(new InstallationContext());
				LogManager.Configuration = configuration;
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
			theEvent.Properties["UserId"] = spider == null ? "DotnetSpider" : spider.UserId;
			theEvent.Properties["TaskGroup"] = spider == null ? "Default" : spider.TaskGroup;
			theEvent.Properties["Identity"] = spider == null ? "Default" : spider.Identity;
			return theEvent;
		}
	}
}
