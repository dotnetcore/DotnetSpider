using System.Threading;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Redial
{
    /// <summary>
    /// 通过进程锁实现的拨号器
    /// </summary>
    public sealed class MutexRedialExecutor : LocalRedialExecutor
	{
		private sealed class MutexLocker : ILocker
		{
			public const string MutexName = "DotnetSpiderRedialLocker";
			private readonly Mutex _syncNamed;

			public MutexLocker()
			{
				bool isOpened = Mutex.TryOpenExisting(MutexName, out _syncNamed);
				if (!isOpened)
				{
					_syncNamed = new Mutex(false, MutexName);
				}
			}

			public void Lock()
			{
				_syncNamed.WaitOne();
			}

			public void Release()
			{
				_syncNamed.ReleaseMutex();
				_syncNamed.Dispose();
			}
		}

		public MutexRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
		}

		public MutexRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		public MutexRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
		}

		protected override bool IsRedialing()
		{
			bool isRedialing = Mutex.TryOpenExisting(MutexLocker.MutexName, out var mutex);
			mutex?.Dispose();
			return isRedialing;
		}

		protected override ILocker CreateLocker()
		{
			return new MutexLocker();
		}

		public override void Dispose()
		{
		}
	}
}
