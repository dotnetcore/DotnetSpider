using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;

namespace DotnetSpider.Redial
{
	public abstract class RedialExecutor : IRedialExecutor
	{
		public IRedialer Redialer { get; }
		public IInternetDetector InternetDetector { get; }
		public abstract void WaitAll();
		public abstract void WaitRedialExit();
		public abstract string CreateActionIdentity(string name);
		public abstract void DeleteActionIdentity(string identity);
		public abstract bool CheckIsRedialing();
		public abstract void ReleaseRedialLocker();
		public abstract void Init();

		protected RedialExecutor(IRedialer redialer, IInternetDetector validater)
		{
			if (redialer == null || validater == null)
			{
				throw new SpiderException("IWaitRedial should not be null.");
			}
			Redialer = redialer;
			InternetDetector = validater;
		}

		public RedialResult Redial()
		{
			if (CheckIsRedialing())
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!CheckIsRedialing())
					{
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
					return RedialResult.Sucess;
				}
				catch (IOException)
				{
					// 有极小可能同时调用File.Open时抛出异常
					return Redial();
				}
				catch (Exception)
				{
					return RedialResult.Failed;
				}
				finally
				{
					ReleaseRedialLocker();
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

		public void Execute(string name, Action<object> action, object obj)
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

		public T Execute<T>(string name, Func<object, T> func, object obj)
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
