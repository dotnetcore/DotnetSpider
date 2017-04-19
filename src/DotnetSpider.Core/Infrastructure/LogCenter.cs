using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogCenter
	{
		private static readonly  ILogger Logger;

		static LogCenter()
		{
			Logger = IocManager.Resolve<ILogger>();
			if (Logger == null)
			{
				Logger = new NLogLogger();
			}
		}

		public static void Log(ITask spider, string message, LogLevel level, Exception e = null)
		{
			Logger.Log(spider, message, level, e);
		}

		public static void Log(this ISpider spider, string message, LogLevel level, Exception e = null)
		{
			Logger.Log(spider, message, level, e);
		}
	}
}
