using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogCenter
	{
		static LogCenter()
		{
#if !NET_CORE
			string nlogConfigPath = Path.Combine(Environment.BaseDirectory, "nlog.net45.config");
#else
			string nlogConfigPath = Path.Combine(Environment.BaseDirectory, "nlog.config");
#endif
			if (!File.Exists(nlogConfigPath))
			{
				using (StreamReader reader = new StreamReader(typeof(LogCenter).Assembly.GetManifestResourceStream("DotnetSpider.Core.nlog.default.config")))
				{
					File.AppendAllText(nlogConfigPath, reader.ReadToEnd());
				}
			}
			XmlLoggingConfiguration configuration = new XmlLoggingConfiguration(nlogConfigPath);
			var connectString = Config.GetValue("connectString");
			var logAndStatusTargets = configuration.AllTargets.Where(t => t.Name == "dblog" || t.Name == "dbstatus").ToList();
			if (!string.IsNullOrEmpty(connectString))
			{
				foreach (var logAndStatusTarget in logAndStatusTargets)
				{
					DatabaseTarget dbTarget = (DatabaseTarget)logAndStatusTarget;
					dbTarget.ConnectionString = connectString;
				}
			}
			else
			{
				var needDeleteRules = configuration.LoggingRules.Where(r => r.Targets.Any(t => t is DatabaseTarget && ((DatabaseTarget)t).ConnectionString == null)).ToList();
				foreach (var rule in needDeleteRules)
				{
					configuration.LoggingRules.Remove(rule);
				}
				configuration.RemoveTarget("dblog");
				configuration.RemoveTarget("dbstatus");
			}

			configuration.Install(new InstallationContext());
			LogManager.Configuration = configuration;
		}

		public static ILogger GetLogger()
		{
			return LogManager.GetCurrentClassLogger();
		}

		public static void MyLog(this ILogger logger, string identity, string message, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, logger.Name, message) { Exception = e };
			theEvent.Properties["Identity"] = identity;
			theEvent.Properties["Node"] = NodeId.Id;
			logger.Log(theEvent);
		}

		public static void MyLog(this ILogger logger, string message, LogLevel level, Exception e = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, logger.Name, message) { Exception = e };
			theEvent.Properties["Identity"] = "System";
			theEvent.Properties["Node"] = NodeId.Id;
			logger.Log(theEvent);
		}
	}
}
