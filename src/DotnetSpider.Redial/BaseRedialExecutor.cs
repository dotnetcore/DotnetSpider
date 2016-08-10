using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Redial.InternetDetector;
using DotnetSpider.Redial.Redialer;
using NLog;

namespace DotnetSpider.Redial
{
	public abstract class BaseRedialExecutor : IRedialExecutor
	{
		public ILogger Logger { get; }

		public IRedialer Redialer { get; }
		public IInternetDetector InternetDetector { get; }
		public abstract void WaitAll();
		public abstract void WaitRedialExit();
		public abstract string CreateActionIdentity(string name);
		public abstract void DeleteActionIdentity(string identity);
		public abstract bool CheckIsRedialing();
		public abstract void ReleaseRedialLocker();

		protected BaseRedialExecutor(IRedialer redialer, IInternetDetector validater)
		{
			if (redialer == null || validater == null)
			{
				throw new SpiderException("IWaitRedial should not be null.");
			}
			Redialer = redialer;
			InternetDetector = validater;
			Logger = LogManager.GetCurrentClassLogger();
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
					Logger.Warn("Wait all atomic action...");

					// 等待数据库等操作完成
					WaitAll();

					Logger.Warn("Start redial...");

					Redialer.Redial();

					InternetDetector.Detect();

					Thread.Sleep(2000);

					Logger.Warn("Redial finished.");
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
