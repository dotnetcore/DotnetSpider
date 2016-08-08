using System;
using System.Threading;

namespace DotnetSpider.Core.Common
{
	public static class RetryExecutor
	{
		public static void Execute(int retryNumber, Action action)
		{
			for (int i = 0; i < retryNumber; ++i)
			{
				try
				{
					action();
					return;
				}
				catch (Exception e)
				{
					// ignored
					Thread.Sleep(500);
				}
			}

			throw new Exception("SafeExecutor failed after times: " + retryNumber);
		}

		public static T Execute<T>(int retryNumber, Func<T> func)
		{
			for (int i = 0; i < retryNumber; ++i)
			{
				try
				{
					return func();
				}
				catch (Exception e)
				{
					Thread.Sleep(500);
					// ignored
				}
			}

			throw new Exception("SafeExecutor failed after times: " + retryNumber);
		}
	}
}
