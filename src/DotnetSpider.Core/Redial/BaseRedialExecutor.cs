using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Redial.Redialer;
using NLog;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Redial
{
	public abstract class RedialExecutor : IRedialExecutor
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();
		protected static object Lock = new object();

		public int WaitRedialTimeout { get; set; } = 100 * 1000 / 100;
		public int RedialIntervalLimit { get; set; } = 10;
		public abstract void WaitAllNetworkRequestComplete();
		public abstract string CreateActionIdentity(string name);
		public abstract void DeleteActionIdentity(string identity);
		public abstract void LockRedial();
		public abstract bool IsRedialing();
		public abstract void ReleaseRedialLock();
		public abstract DateTime GetLastRedialTime();
		public abstract void RecordLastRedialTime();

		public IRedialer Redialer { get; }

		public IInternetDetector InternetDetector { get; }

		public int AfterRedialWaitTime { get; set; } = -1;

		public void WaitRedialComplete()
		{
			for (int i = 0; i < WaitRedialTimeout; ++i)
			{
				if (IsRedialing())
				{
					Thread.Sleep(100);
				}
				else
				{
					return;
				}
			}
			throw new RedialException("Wait redial timeout.");
		}

		public RedialResult Redial(Action action = null)
		{
			try
			{
				Logger.MyLog("Try to lock redialer...", LogLevel.Warn);
				LockRedial();
				Logger.MyLog("Lock redialer", LogLevel.Warn);
				var lastRedialTime = GetLastRedialTime();
				if ((DateTime.Now - lastRedialTime).Seconds < RedialIntervalLimit)
				{
					return RedialResult.OtherRedialed;
				}
				Thread.Sleep(500);
				Logger.MyLog("Wait all network requests complete...", LogLevel.Warn);
				// 等待所有网络请求结束
				WaitAllNetworkRequestComplete();
				Logger.MyLog("Start redial...", LogLevel.Warn);

				var redialCount = 1;

				while (redialCount++ < 10)
				{
					try
					{
						Console.WriteLine($"redial loop {redialCount}");
						Redialer.Redial();

						if (InternetDetector.Detect())
						{
							Console.WriteLine($"redial loop {redialCount} success.");
							break;
						}
						else
						{
							Console.WriteLine($"redial loop {redialCount} failed.");
						}
					}
					catch (Exception ex)
					{
						Logger.MyLog($"Redial failed loop {redialCount}: {ex}", LogLevel.Error);
						continue;
					}
				}

				if (redialCount > 10)
				{
					return RedialResult.Failed;
				}

				Logger.MyLog("Redial success.", LogLevel.Warn);

				action?.Invoke();

				return RedialResult.Sucess;
			}
			catch (Exception ex)
			{
				Logger.MyLog($"Redial failed: {ex}", LogLevel.Error);
				return RedialResult.Failed;
			}
			finally
			{
				ReleaseRedialLock();
				if (AfterRedialWaitTime > 0)
				{
					Console.WriteLine($"Going to sleep for {AfterRedialWaitTime} after redial.");
					Thread.Sleep(AfterRedialWaitTime);
				}
			}
		}

		public void Execute(string name, Action action)
		{
			WaitRedialComplete();

			string identity = null;
			try
			{
				identity = CreateActionIdentity(name);
				action();
			}
			finally
			{
				DeleteActionIdentity(identity);
			}
		}

		public void Execute(string name, Action<dynamic> action, dynamic obj)
		{
			WaitRedialComplete();

			string identity = null;
			try
			{
				identity = CreateActionIdentity(name);
				action(obj);
			}
			finally
			{
				DeleteActionIdentity(identity);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			WaitRedialComplete();

			string identity = null;
			try
			{
				identity = CreateActionIdentity(name);
				return func();
			}
			finally
			{
				DeleteActionIdentity(identity);
			}
		}

		public T Execute<T>(string name, Func<dynamic, T> func, dynamic obj)
		{
			WaitRedialComplete();

			string identity = null;
			try
			{
				identity = CreateActionIdentity(name);
				return func(obj);
			}
			finally
			{
				DeleteActionIdentity(identity);
			}
		}

		protected RedialExecutor(IRedialer redialer, IInternetDetector validater)
		{
			if (redialer == null || validater == null)
			{
				throw new RedialException("IRedialer, validatershould not be null.");
			}
			Redialer = redialer;
			InternetDetector = validater;
		}
	}
}
