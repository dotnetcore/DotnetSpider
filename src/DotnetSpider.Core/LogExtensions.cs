using NLog;

namespace DotnetSpider.Core
{
	public static class LogExtensions
	{
		public static void SaveLog(this ILogger logger, LogEventInfo logInfo)
		{
			NetworkCenter.Current.Execute("log", () =>
			{
				logger.Log(logInfo);
			});
		}
	}
}
