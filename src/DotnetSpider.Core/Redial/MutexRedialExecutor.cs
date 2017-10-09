using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Redial.Redialer;
using System.Linq;
using DotnetSpider.Core.Redial.InternetDetector;
using System.Collections.Concurrent;
using DotnetSpider.Core.Infrastructure;
using LogLevel = NLog.LogLevel;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Redial
{
	public sealed class MutexRedialExecutor : LocalRedialExecutor
	{
		public sealed class MutexLocker : ILocker
		{
			public const string MutexName = "DotnetSpiderRedialLocker";
			private Mutex SyncNamed;

			public MutexLocker()
			{
				bool isOpened = Mutex.TryOpenExisting(MutexName, out SyncNamed);
				if (!isOpened)
				{
					SyncNamed = new Mutex(false, MutexName);
				}
			}

			public void Lock()
			{
				SyncNamed.WaitOne();
			}

			public void Release()
			{
				SyncNamed.ReleaseMutex();
				SyncNamed.Dispose();
			}
		}

		public MutexRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
		}

		public MutexRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		public MutexRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
		}

		public override bool IsRedialing()
		{
			Mutex mutex;
			bool isRedialing = Mutex.TryOpenExisting(MutexLocker.MutexName, out mutex);
			mutex?.Dispose();
			return isRedialing;
		}

		public override ILocker CreateLocker()
		{
			return new MutexLocker();
		}

		public override void Dispose()
		{
		}
	}
}
