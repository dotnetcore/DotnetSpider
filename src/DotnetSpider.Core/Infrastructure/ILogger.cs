using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// Defines available log levels.
	/// </summary>
	public sealed class Level
	{
		/// <summary>
		/// Gets the name of the log level.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the ordinal of the log level.
		/// </summary>
		public int Ordinal { get; private set; }

		/// <summary>
		/// Trace log level.
		/// </summary>
		public static readonly Level Trace = new Level { Name = LogLevel.Trace.Name, Ordinal = LogLevel.Trace.Ordinal };

		/// <summary>
		/// Debug log level.
		/// </summary>
		public static readonly Level Debug = new Level { Name = LogLevel.Debug.Name, Ordinal = LogLevel.Debug.Ordinal };

		/// <summary>
		/// Info log level.
		/// </summary>
		public static readonly Level Info = new Level { Name = LogLevel.Info.Name, Ordinal = LogLevel.Info.Ordinal };

		/// <summary>
		/// Warn log level.
		/// </summary>
		public static readonly Level Warn = new Level { Name = LogLevel.Warn.Name, Ordinal = LogLevel.Warn.Ordinal };

		/// <summary>
		/// Error log level.
		/// </summary>
		public static readonly Level Error = new Level { Name = LogLevel.Error.Name, Ordinal = LogLevel.Error.Ordinal };

		/// <summary>
		/// Fatal log level.
		/// </summary>
		public static readonly Level Fatal = new Level { Name = LogLevel.Fatal.Name, Ordinal = LogLevel.Fatal.Ordinal };

		/// <summary>
		/// Off log level.
		/// </summary>
		public static readonly Level Off = new Level { Name = LogLevel.Off.Name, Ordinal = LogLevel.Off.Ordinal };

		internal LogLevel ToNLogLevel()
		{
			return LogLevel.FromOrdinal(this.Ordinal);
		}
	}

	/// <summary>
	/// Provides logging interface and utility functions.
	/// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// 通过NLog、Http写日志.
		/// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
		/// </summary>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void Log(string message, Level level, Exception exception = null);

		/// <summary>
		/// 通过NLog、Http写日志.
		/// Http日志的开关为: Env.EnterpiseService &amp;&amp; Env.EnterpiseServiceLog
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void Log(string identity, string message, Level level, Exception exception = null);

		/// <summary>
		/// 仅使用NLog写日志
		/// </summary>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void NLog(string message, Level level, Exception exception = null);

		/// <summary>
		/// 仅使用NLog写日志
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void NLog(string identity, string message, Level level, Exception exception = null);

		/// <summary>
		/// 仅使用HTTP写日志
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void HttpLog(string identity, string message, Level level, Exception exception = null);

		/// <summary>
		/// 仅使用HTTP写日志
		/// </summary>
		/// <param name="message">信息</param>
		/// <param name="level">日志级别</param>
		/// <param name="exception">异常信息</param>
		void HttpLog(string message, Level level, Exception exception = null);
	}
}
