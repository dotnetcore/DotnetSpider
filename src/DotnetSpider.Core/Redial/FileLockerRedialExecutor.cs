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
	public class FileLockerRedialExecutor : LocalRedialExecutor
	{
		public sealed class FileLocker : ILocker
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
						continue;
					}
				}
			}

			public void Release()
			{
				_lockStream.Dispose();
				File.Delete(RedialLockerFile);
			}
		}

		public FileLockerRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		public FileLockerRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
		}

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
				}
			}
		}

		public override bool IsRedialing()
		{
			return File.Exists(FileLocker.RedialLockerFile);
		}

		public override void Dispose()
		{
		}

		public override ILocker CreateLocker()
		{
			return new FileLocker();
		}
	}
}
