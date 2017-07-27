using NLog;
using System;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure
{
	public static class RetryExecutor
	{
		private readonly static ILogger Logger = LogCenter.GetLogger();

		public static void Execute(int retryNumber, Action action)
		{
			for (int i = 0; i < retryNumber; ++i)
			{
				try
				{
					action();
					return;
				}
				catch (Exception ex)
				{
					Logger.MyLog("Retry action failed.", LogLevel.Error, ex);
					Thread.Sleep(500);
				}
			}

			throw new SpiderException($"SafeExecutor failed after times: {retryNumber}.");
		}

		public static T Execute<T>(int retryNumber, Func<T> func)
		{
			for (int i = 0; i < retryNumber; ++i)
			{
				try
				{
					return func();
				}
				catch (Exception ex)
				{
					Logger.MyLog("Retry action failed.", LogLevel.Error, ex);
					Thread.Sleep(500);
				}
			}
			throw new SpiderException($"SafeExecutor failed after times: {retryNumber}.");
		}
	}
}
