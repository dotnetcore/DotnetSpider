using System;
using System.Threading;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Redial.Redialer;
using NLog;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Redial
{
	/// <summary>
	/// <see cref="IRedialExecutor"/>
	/// </summary>
	public abstract class RedialExecutor : IRedialExecutor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		internal bool IsTest { get; set; }
		private int WaitRedialTimeout { get; } = 100 * 1000 / 100;
		private int RedialIntervalLimit { get; } = 10;

		/// <summary>
		/// 等待所有网络通讯结束
		/// </summary>
		public abstract void WaitAllNetworkRequestComplete();

		/// <summary>
		/// 创建通讯标识
		/// </summary>
		/// <param name="name">通讯标识前缀</param>
		/// <returns>通讯标识</returns>
		public abstract string CreateActionIdentity(string name);

		/// <summary>
		/// 删除通讯标识
		/// </summary>
		/// <param name="identity">通讯标识</param>
		public abstract void DeleteActionIdentity(string identity);

		/// <summary>
		/// 创建同步锁
		/// </summary>
		/// <returns>同步锁</returns>
		protected abstract ILocker CreateLocker();

		/// <summary>
		/// 判断是否有别的程序正在拨号
		/// </summary>
		/// <returns>是否有别的程序正在拨号, 如果有返回 True, 没有则返回 False.</returns>
		protected abstract bool IsRedialing();

		/// <summary>
		/// 取得上次拨号的时间, 如果间隔太短则不执行拨号操作
		/// </summary>
		/// <returns>上次拨号的时间</returns>
		protected abstract DateTime GetLastRedialTime();

		/// <summary>
		/// 记录拨号时间
		/// </summary>
		protected abstract void RecordLastRedialTime();

		public abstract void Dispose();

		public int AfterRedialWaitTime { get; set; } = -1;

		public RedialResult Redial(Action action = null)
		{
			ILocker locker = null;
			try
			{
				Logger.NLog("Try to lock redialer...", LogLevel.Info);
				locker = CreateLocker();
				locker.Lock();
				Logger.NLog("Lock redialer", LogLevel.Info);
				var lastRedialTime = (DateTime.Now - GetLastRedialTime()).Seconds;
				if (lastRedialTime < RedialIntervalLimit)
				{
					Logger.NLog($"Skip redial because last redial compeleted before {lastRedialTime} seconds.", LogLevel.Info);
					return RedialResult.OtherRedialed;
				}
				Thread.Sleep(500);
				Logger.NLog("Wait all network requests complete...", LogLevel.Info);
				// 等待所有网络请求结束
				WaitAllNetworkRequestComplete();
				Logger.NLog("Start redial...", LogLevel.Info);

				var redialCount = 1;

				while (redialCount++ < 10)
				{
					try
					{
						Console.WriteLine($"redial loop {redialCount}");

						if (!IsTest)
						{
							Redialer.Redial();
						}

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
						Logger.NLog($"Redial failed loop {redialCount}: {ex}", LogLevel.Error);
					}
				}

				if (redialCount > 10)
				{
					return RedialResult.Failed;
				}

				Logger.NLog("Redial success.", LogLevel.Info);

				action?.Invoke();

				RecordLastRedialTime();

				return RedialResult.Sucess;
			}
			catch (Exception ex)
			{
				Logger.NLog($"Redial failed: {ex}", LogLevel.Error);
				return RedialResult.Failed;
			}
			finally
			{
				locker?.Release();
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
				throw new RedialException("IRedialer, validatershould should not be null.");
			}
			Redialer = redialer;
			InternetDetector = validater;
		}

		protected RedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector())
		{
		}

		private IRedialer Redialer { get; }

		private IInternetDetector InternetDetector { get; }

		private void WaitRedialComplete()
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
	}
}
