using NLog;
using System;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure
{
	public static class RetryExecutor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

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
					Logger.AllLog($"Retry action failed: {ex}", LogLevel.Error);
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
					Logger.AllLog($"Retry action failed: {ex}", LogLevel.Error);
					Thread.Sleep(500);
				}
			}
			throw new SpiderException($"SafeExecutor failed after times: {retryNumber}.");
		}
	}
}
