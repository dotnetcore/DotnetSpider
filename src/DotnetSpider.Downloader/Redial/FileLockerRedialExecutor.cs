using DotnetSpider.Downloader.Redial.InternetDetector;
using DotnetSpider.Downloader.Redial.Redialer;
using System.IO;
using System.Threading;

namespace DotnetSpider.Downloader.Redial
{
	/// <summary>
	/// 通过文件锁锁实现的拨号器
	/// </summary>
	public class FileLockerRedialExecutor : LocalRedialExecutor
	{
		private sealed class FileLocker : ILocker
		{
			public static readonly string RedialLockerFile = Path.Combine(DownloaderEnv.GlobalDirectory, "redial.lock");
			private Stream _lockStream;

			public FileLocker()
			{
				var fileInfo = new FileInfo(RedialLockerFile);
				if (fileInfo.Exists)
				{
					try
					{
						fileInfo.Delete();
					}
					catch
					{
						// ignored
					}
				}
			}

			public void Lock()
			{
				while (true)
				{
					try
					{
						_lockStream = File.Create(RedialLockerFile);
						return;
					}
					catch
					{
						Thread.Sleep(50);
					}
				}
			}

			public void Release()
			{
				_lockStream.Dispose();

				for (int i = 0; i < 3; ++i)
				{
					try
					{
						File.Delete(RedialLockerFile);
					}
					catch
					{
						// ignored
					}
				}
			}
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		public FileLockerRedialExecutor() : this(new DefaultAdslRedialer(), new DefaultInternetDetector())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="redialer">拨号器</param>
		public FileLockerRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="redialer">拨号器</param>
		/// <param name="validater">网络状态检测器</param>
		public FileLockerRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
			var fileInfo = new FileInfo(FileLocker.RedialLockerFile);
			if (fileInfo.Exists)
			{
				try
				{
					fileInfo.Delete();
				}
				catch
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// 判断是否有别的程序正在拨号
		/// </summary>
		/// <returns>是否有别的程序正在拨号, 如果有返回 True, 没有则返回 False.</returns>
		protected override bool IsRedialing()
		{
			return File.Exists(FileLocker.RedialLockerFile);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// 创建同步锁
		/// </summary>
		/// <returns>同步锁</returns>
		protected override ILocker CreateLocker()
		{
			return new FileLocker();
		}
	}
}
