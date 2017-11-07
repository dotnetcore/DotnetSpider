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

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogCenter
	{
		private static bool _submitHttpLog;
		private static readonly HttpClient _httpClient = new HttpClient();

		static LogCenter()
		{
			InitLogCenter();
			_submitHttpLog = !string.IsNullOrEmpty(Env.HttpLogCenter);
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

			SubmitHttpLog(identity, message, level, e);
		}

		public static void MyLog(this ILogger logger, string message, LogLevel level, Exception e = null)
		{
			MyLog(logger, "System", message, level, e);
		}

		private static void SubmitHttpLog(string identity, string message, LogLevel level, Exception e = null)
		{
			if (_submitHttpLog)
			{
				var json = JsonConvert.SerializeObject(new
				{
					Token = Env.HttpLogCenterToken,
					Identity = identity,
					LogInfo = new
					{
						Identity = identity,
						Node = NodeId.Id,
						Logged = DateTime.UtcNow,
						Level = level.ToString(),
						Message = message,
						Exception = e?.ToString(),
					}
				});
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				try
				{
					NetworkCenter.Current.Execute("log", () =>
					{
						_httpClient.PostAsync(Env.HttpLogCenter, content).Wait();
					});
				}
				catch
				{
				}
			}
		}
	}
}
