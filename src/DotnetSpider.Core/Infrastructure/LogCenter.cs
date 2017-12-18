using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using DotnetSpider.Core.Redial;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogCenter
	{
		private static ILogger Logger;

		static LogCenter()
		{
			InitLogCenter();
		}

		public static ILogger GetLogger()
		{
			return LogManager.GetLogger("DotnetSpider");
		}

		public static void InitLogCenter()
		{
#if !NET_CORE
			string nlogConfigPath = Path.Combine(Env.BaseDirectory, "nlog.net45.config");
#else
			string nlogConfigPath = Path.Combine(Env.BaseDirectory, "nlog.config");
#endif
			if (!File.Exists(nlogConfigPath))
			{
				File.AppendAllText(nlogConfigPath, GetDefaultConfigString());
			}
			XmlLoggingConfiguration configuration = new XmlLoggingConfiguration(nlogConfigPath);

			if (Env.SystemConnectionStringSettings == null)
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
					dbTarget.ConnectionString = Env.SystemConnectionStringSettings.ConnectionString;
				}
			}

			configuration.Install(new InstallationContext());
			LogManager.Configuration = configuration;
			Logger = GetLogger();
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

		public static void NLog(this ILogger logger, string identity, string message, LogLevel level, Exception exception = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, logger.Name, message) { Exception = exception };
			theEvent.Properties["Identity"] = identity;
			theEvent.Properties["NodeId"] = NodeId.Id;
			logger.Log(theEvent);
		}

		public static void NLog(this ILogger logger, string message, LogLevel level, Exception exception = null)
		{
			NLog(logger, "System", message, level, exception);
		}

		public static void AllLog(this ILogger logger, string identity, string message, LogLevel level, Exception exception = null)
		{
			NLog(logger, identity, message, level, exception);

			HttpLog(identity, message, level, exception);
		}

		public static void AllLog(this ILogger logger, string message, LogLevel level, Exception exception = null)
		{
			AllLog(logger, "System", message, level, exception);
		}

		private static void HttpLog(string identity, string message, LogLevel level, Exception exception = null)
		{
			if (Env.SubmitHttpLog)
			{
				var json = JsonConvert.SerializeObject(new
				{
					Token = Env.HttpCenterToken,
					Identity = identity,
					LogInfo = new
					{
						Identity = identity,
						NodeId = NodeId.Id,
						Logged = DateTime.UtcNow,
						Level = level.ToString(),
						Message = message,
						Exception = exception?.ToString(),
					}
				});
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				for (int i = 0; i < 10; ++i)
				{
					try
					{
						NetworkCenter.Current.Execute("log", () =>
						{
							var response = HttpSender.Client.PostAsync(Env.HttpLogUrl, content).Result;
							response.EnsureSuccessStatusCode();
						});
						break;

					}
					catch (Exception ex)
					{
						Logger.Error($"Submit log failed [{i}]: {ex}");
						Thread.Sleep(5000);
					}
				}
			}
		}
	}
}
