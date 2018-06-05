using System.IO;
using System.Threading;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Redial
{
    /// <summary>
    /// 通过文件锁锁实现的拨号器
    /// </summary>
    public class FileLockerRedialExecutor : LocalRedialExecutor
	{
		private sealed class FileLocker : ILocker
		{
			public static readonly string RedialLockerFile = Path.Combine(Env.GlobalDirectory, "redial.lock");
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
					}
				}
			}
		}

		public FileLockerRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
		}

		public FileLockerRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

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

		protected override bool IsRedialing()
		{
			return File.Exists(FileLocker.RedialLockerFile);
		}

		public override void Dispose()
		{
		}

		protected override ILocker CreateLocker()
		{
			return new FileLocker();
		}
	}
}
