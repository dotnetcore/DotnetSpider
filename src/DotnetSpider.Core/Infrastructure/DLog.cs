using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
    /// <summary>
    /// DotnetSpider 日志实现
    /// </summary>
    public class DLog : ILogger
    {
        private static NLog.ILogger _nlogger;
        private const string SystemIdentity = "System";
        private static RetryPolicy RetryPolicy = Policy.Handle<Exception>().Retry(5, (ex, count) =>
        {
            _nlogger.Error($"Submit http log failed [{count}]: {ex}");
        });

        private DLog() { }

        static DLog()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// 取得日志接口
        /// </summary>
        /// <returns>日志接口</returns>
        public static ILogger GetLogger()
        {
            return new DLog();
        }

        /// <summary>
        /// 通过NLog、Http写日志.
        /// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void Log(string message, Level level, Exception exception = null)
        {
            Log(SystemIdentity, message, level, exception);
        }

        /// <summary>
        /// 通过NLog、Http写日志.
        /// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
        /// </summary>
        /// <param name="identity">唯一标识</param>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void Log(string identity, string message, Level level, Exception exception = null)
        {
            NLog(identity, message, level, exception);
            HttpLog(identity, message, level, exception);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Trace(string message, Exception exception = null)
        {
            Log(message, Level.Trace, exception);
        }
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Debug(string message, Exception exception = null)
        {
            Log(message, Level.Debug, exception);
        }
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Info(string message, Exception exception = null)
        {
            Log(message, Level.Info, exception);
        }
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Warn(string message, Exception exception = null)
        {
            Log(message, Level.Warn, exception);
        }
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Error(string message, Exception exception = null)
        {
            Log(message, Level.Error, exception);
        }
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Fatal(string message, Exception exception = null)
        {
            Log(message, Level.Fatal, exception);
        }

        /// <summary>
        /// 仅使用NLog写日志
        /// </summary>
        /// <param name="identity">唯一标识</param>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void NLog(string identity, string message, Level level, Exception exception = null)
        {
            LogEventInfo theEvent = new LogEventInfo(level.ToNLogLevel(), _nlogger.Name, message) { Exception = exception };
            theEvent.Properties["Identity"] = identity;
            theEvent.Properties["NodeId"] = Env.NodeId;
            _nlogger.Log(theEvent);
        }

        /// <summary>
        /// 仅使用NLog写日志
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void NLog(string message, Level level, Exception exception = null)
        {
            NLog(SystemIdentity, message, level, exception);
        }

        /// <summary>
        /// 仅使用HTTP写日志
        /// </summary>
        /// <param name="identity">唯一标识</param>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void HttpLog(string identity, string message, Level level, Exception exception = null)
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
                        Env.NodeId,
                        Logged = DateTime.UtcNow,
                        Level = level.Name,
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

        /// <summary>
        /// 仅使用HTTP写日志
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="level">日志级别</param>
        /// <param name="exception">异常信息</param>
        public void HttpLog(string message, Level level, Exception exception = null)
        {
            HttpLog(SystemIdentity, message, level, exception);
        }

        /// <summary>
        /// 取得默认的NLog配置内容
        /// </summary>
        /// <returns>NLog配置内容</returns>
        internal static string GetDefaultConfigString()
        {
            var stream = typeof(DLog).Assembly.GetManifestResourceStream("DotnetSpider.Core.nlog.default.config");
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
        /// 加载配置文件
        /// </summary>
        private static void LoadConfiguration()
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
            _nlogger = LogManager.GetLogger("DotnetSpider");
        }
    }
}
