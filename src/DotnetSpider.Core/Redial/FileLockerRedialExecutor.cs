using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Redial.Redialer;
using System.Linq;
using DotnetSpider.Core.Redial.InternetDetector;
using System.Collections.Concurrent;

namespace DotnetSpider.Core.Redial
{
	public class FileLockerRedialExecutor : RedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialLockerFile;
		private static readonly int RedialTimeout = 120 * 1000 / 50;

		private static ConcurrentDictionary<string, Stream> files = new ConcurrentDictionary<string, Stream>();

		static FileLockerRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(Infrastructure.Environment.GlobalDirectory, "atomicaction");
			RedialLockerFile = Path.Combine(Infrastructure.Environment.GlobalDirectory, "redial.lock");
		}

		public FileLockerRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
			if (!Directory.Exists(AtomicActionFolder))
			{
				Directory.CreateDirectory(AtomicActionFolder);
			}

			foreach (var file in Directory.GetFiles(AtomicActionFolder))
			{
				try
				{
					File.Delete(file);
				}
				catch
				{
				}
			}
			try
			{
				File.Delete(RedialLockerFile);
			}
			catch
			{
			}
		}

		public override void WaitAll()
		{
			files.TryAdd(RedialLockerFile, File.Create(RedialLockerFile));
			while (true)
			{
				if (!Directory.GetFiles(AtomicActionFolder).Any())
				{
					break;
				}
				Thread.Sleep(50);
			}
		}

		public override void WaitRedialExit()
		{
			lock (this)
			{
				if (File.Exists(RedialLockerFile))
				{
					for (int i = 0; i < RedialTimeout; ++i)
					{
						Thread.Sleep(50);
						if (!File.Exists(RedialLockerFile))
						{
							break;
						}
					}
				}
			}
		}

		public override string CreateActionIdentity(string name)
		{
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			files.TryAdd(id, File.Create(id));
			return id;
		}

		public override void DeleteActionIdentity(string identity)
		{
			Stream stream;
			files.TryRemove(identity, out stream);
			if (stream != null)
			{
				stream.Dispose();
			}
			File.Delete(identity);
		}

		public override bool CheckIsRedialing()
		{
			lock (Lock)
			{
				if (!File.Exists(RedialLockerFile))
				{
					//File.Create(RedialLockerFile).Dispose();
					return false;
				}
				return true;
			}
		}

		public override void ReleaseRedialLocker()
		{
			Stream stream;
			files.TryRemove(RedialLockerFile, out stream);
			if (stream != null)
			{
				stream.Dispose();
			}
			File.Delete(RedialLockerFile);
		}
	}
}
