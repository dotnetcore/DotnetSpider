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
using Polly.Retry;
using Polly;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 日志中心
	/// </summary>
	public static class LogCenter
	{
		private static ILogger _logger;

		private static readonly RetryPolicy RetryPolicy = Policy.Handle<Exception>().Retry(5, (ex, count) =>
		{
			_logger.Error($"Submit http log failed [{count}]: {ex}");
		});

		static LogCenter()
		{
			InitLogCenter();
		}

		/// <summary>
		/// 取得日志接口
		/// </summary>
		/// <returns>日志接口</returns>
		public static ILogger GetLogger()
		{
			return LogManager.GetLogger("DotnetSpider");
		}

		/// <summary>
		/// 初始化日志中心
		/// </summary>
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
			_logger = GetLogger();
		}

		/// <summary>
		/// 取得默认的NLog配置内容
		/// </summary>
		/// <returns>NLog配置内容</returns>
		internal static string GetDefaultConfigString()
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

		/// <summary>
		/// 仅使用NLog写日志
		/// </summary>
		/// <param name="logger">日志接口</param>
		/// <param name="identity">唯一标识</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		public static void NLog(this ILogger logger, string identity, string message, LogLevel level, Exception exception = null)
		{
			LogEventInfo theEvent = new LogEventInfo(level, logger.Name, message) { Exception = exception };
			theEvent.Properties["Identity"] = identity;
			theEvent.Properties["NodeId"] = Env.NodeId;
			logger.Log(theEvent);
		}

		/// <summary>
		/// 仅使用NLog写日志
		/// </summary>
		/// <param name="logger">日志接口</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		public static void NLog(this ILogger logger, string message, LogLevel level, Exception exception = null)
		{
			NLog(logger, "System", message, level, exception);
		}

		/// <summary>
		/// 通过NLog、和Http写日志.
		/// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
		/// </summary>
		/// <param name="logger">日志接口</param>
		/// <param name="identity">唯一标识</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		public static void AllLog(this ILogger logger, string identity, string message, LogLevel level, Exception exception = null)
		{
			NLog(logger, identity, message, level, exception);

			HttpLog(identity, message, level, exception);
		}

		/// <summary>
		/// 通过NLog、和Http写日志.
		/// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
		/// </summary>
		/// <param name="logger">日志接口</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		public static void AllLog(this ILogger logger, string message, LogLevel level, Exception exception = null)
		{
			AllLog(logger, "System", message, level, exception);
		}

		private static void HttpLog(string identity, string message, LogLevel level, Exception exception = null)
		{
			if (Env.EnterpiseService && Env.EnterpiseServiceLog)
			{
				var json = JsonConvert.SerializeObject(new
				{
					Token = Env.EnterpiseServiceToken,
					Identity = identity,
					LogInfo = new
					{
						Identity = identity,
						NodeId = Env.NodeId,
						Logged = DateTime.UtcNow,
						Level = level.ToString(),
						Message = message,
						Exception = exception?.ToString(),
					}
				});
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				RetryPolicy.ExecuteAndCapture(() =>
				{
					NetworkCenter.Current.Execute("log", () =>
					{
						var response = HttpSender.Client.PostAsync(Env.EnterpiseServiceLogUrl, content).Result;
						response.EnsureSuccessStatusCode();
					});
				});
			}
		}
	}
}
