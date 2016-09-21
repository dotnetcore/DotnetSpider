using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core
{
	public static class LogCenter
	{
		private static readonly List<ILogger> Loggers;

		static LogCenter()
		{
			Loggers = IocContainer.Default.GetServices<ILogger>().ToList();
			if (Loggers.Count == 0)
			{
				Loggers = new List<ILogger> { new NLogLogger() };
			}
		}

		public static void Log(ITask spider, string message, LogLevel level, Exception e = null)
		{
			foreach (var logger in Loggers)
			{
				logger.Log(spider, message, level, e);
			}
		}

		public static void Log(this ISpider spider, string message, LogLevel level, Exception e = null)
		{
			foreach (var logger in Loggers)
			{
				logger.Log(spider, message, level, e);
			}
		}
	}
}
