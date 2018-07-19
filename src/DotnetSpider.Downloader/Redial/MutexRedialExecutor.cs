using DotnetSpider.Downloader.Redial.InternetDetector;
using DotnetSpider.Downloader.Redial.Redialer;
using System.Threading;

namespace DotnetSpider.Downloader.Redial
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
				try
				{
					_syncNamed = Mutex.OpenExisting(MutexName);
				}
				catch
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
#if NET40
				_syncNamed.Close();
#else
				_syncNamed.Dispose();
#endif
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
		public MutexRedialExecutor() : this(new DefaultAdslRedialer(), new DefaultInternetDetector())
		{
		}

		/// <summary>
		/// 判断是否有别的程序正在拨号
		/// </summary>
		/// <returns>是否有别的程序正在拨号, 如果有返回 True, 没有则返回 False.</returns>
		protected override bool IsRedialing()
		{
			Mutex mutex = null;
			try
			{
				mutex = Mutex.OpenExisting(MutexLocker.MutexName);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
#if NETFRAMEWORK
				mutex?.Close();
#else
				mutex?.Dispose();
#endif
			}
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
