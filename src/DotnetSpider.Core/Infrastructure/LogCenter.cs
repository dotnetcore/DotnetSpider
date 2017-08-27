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
			InitLogCenter();
		}

		public static ILogger GetLogger()
		{
			return LogManager.GetCurrentClassLogger();
		}

		public static void InitLogCenter()
		{
#if !NET_CORE
			string nlogConfigPath = Path.Combine(Environment.BaseDirectory, "nlog.net45.config");
#else
			string nlogConfigPath = Path.Combine(Environment.BaseDirectory, "nlog.config");
#endif
			if (!File.Exists(nlogConfigPath))
			{
				File.AppendAllText(nlogConfigPath, GetDefaultConfigString());
			}
			XmlLoggingConfiguration configuration = new XmlLoggingConfiguration(nlogConfigPath);

			if (Environment.SystemConnectionStringSettings == null)
			{
				var needDeleteRules = configuration.LoggingRules.Where(r => r.Targets.Any(t => t is DatabaseTarget && ((DatabaseTarget)t).ConnectionString == null)).ToList();
				foreach (var rule in needDeleteRules)
				{
					configuration.LoggingRules.Remove(rule);
				}
				configuration.RemoveTarget("dblog");
			}
			else
			{
				var dblog = configuration.AllTargets.FirstOrDefault(t => t.Name == "dblog");
				if (dblog != null)
				{
					DatabaseTarget dbTarget = (DatabaseTarget)dblog;
					dbTarget.ConnectionString = Environment.SystemConnectionStringSettings.ConnectionString;
				}
			}

			configuration.Install(new InstallationContext());
			LogManager.Configuration = configuration;
		}

		public static string GetDefaultConfigString()
		{
			var stream = typeof(LogCenter).Assembly.GetManifestResourceStream("DotnetSpider.Core.nlog.default.config");
			if (stream == null)
			{
				return string.Empty;
			}
			else
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
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
