using System;
using System.IO;
using System.Threading;
using DotnetSpider.Extension.Redial.InternetDetector;
using DotnetSpider.Extension.Redial.Redialer;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Redial
{
	public abstract class RedialExecutor : IRedialExecutor
	{
		protected static object Lock = new object();
		public IRedialer Redialer { get; }
		public IInternetDetector InternetDetector { get; }
		public abstract void WaitAll();
		public abstract void WaitRedialExit();
		public abstract string CreateActionIdentity(string name);
		public abstract void DeleteActionIdentity(string identity);
		public abstract bool CheckIsRedialing();
		public abstract void ReleaseRedialLocker();

		public int AfterRedialWaitTime { get; set; } = -1;

		protected RedialExecutor(IRedialer redialer, IInternetDetector validater)
		{
			if (redialer == null || validater == null)
			{
				throw new RedialException("IRedialer, validatershould not be null.");
			}
			Redialer = redialer;
			InternetDetector = validater;
		}

		public RedialResult Redial(Action action = null)
		{
			if (CheckIsRedialing())
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!CheckIsRedialing())
					{
						//ReleaseRedialLocker();
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				try
				{
					LogCenter.Log(null, "Wait all atomic action...", LogLevel.Warn);
					// 等待数据库等操作完成
					WaitAll();
					LogCenter.Log(null, "Start redial...", LogLevel.Warn);

					Redialer.Redial();

					InternetDetector.Detect();

					Thread.Sleep(2000);
					LogCenter.Log(null, "Redial finished.", LogLevel.Warn);

					action?.Invoke();

					return RedialResult.Sucess;
				}
				catch (IOException)
				{
					// 有极小可能同时调用File.Open时抛出异常
					return Redial(action);
				}
				catch (Exception)
				{
					return RedialResult.Failed;
				}
				finally
				{
					ReleaseRedialLocker();
					if (AfterRedialWaitTime > 0)
					{
						Console.WriteLine($"Going to sleep for {AfterRedialWaitTime} after redial.");
						Thread.Sleep(AfterRedialWaitTime);
					}
				}
			}
		}

		public void Execute(string name, Action action)
		{
			WaitRedialExit();

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
			WaitRedialExit();

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
			WaitRedialExit();

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
			WaitRedialExit();

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
	}
}
