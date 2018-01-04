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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="redialer">拨号器</param>
		/// <param name="validater">网络状态检测器</param>
		public MutexRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="redialer">拨号器</param>
		public MutexRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		/// <summary>
		/// 构造方法
		/// </summary>
		public MutexRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
		}

		/// <summary>
		/// 判断是否有别的程序正在拨号
		/// </summary>
		/// <returns>是否有别的程序正在拨号, 如果有返回 True, 没有则返回 False.</returns>
		protected override bool IsRedialing()
		{
			bool isRedialing = Mutex.TryOpenExisting(MutexLocker.MutexName, out var mutex);
			mutex?.Dispose();
			return isRedialing;
		}

		/// <summary>
		/// 创建同步锁
		/// </summary>
		/// <returns>同步锁</returns>
		protected override ILocker CreateLocker()
		{
			return new MutexLocker();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
		}
	}
}
