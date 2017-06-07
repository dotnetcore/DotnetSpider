using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogCenter
	{
		private static readonly ILogger Logger;
 
		static LogCenter()
		{
			Logger = IocManager.Resolve<ILogger>() ?? new NLogLogger();
		}

		public static void Log(this IIdentity identity, string message, LogLevel level, Exception e = null)
		{
			Logger.Log(identity, NodeId.Id, message, level, e);
		}
	}
}
