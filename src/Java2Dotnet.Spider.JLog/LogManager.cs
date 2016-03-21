
using System.Collections.Generic;

namespace Java2Dotnet.Spider.JLog
{
	public static class LogManager
	{
		private static readonly Dictionary<string,ILog> LoggerCahced=new Dictionary<string, ILog>();

		public static ILog GetLogger(string name = null)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = "DEFAULT";
			}

			if (LoggerCahced.ContainsKey(name))
			{
				return LoggerCahced[name];
			}
			else
			{
				var logger = new Log(name);
				LoggerCahced.Add(name,logger);
				return logger;
			}
		}
	}
}
