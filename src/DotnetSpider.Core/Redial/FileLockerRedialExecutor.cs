using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Redial.Redialer;
using System.Linq;
using DotnetSpider.Core.Redial.InternetDetector;
using System.Collections.Concurrent;
using DotnetSpider.Core.Infrastructure;
using LogLevel = NLog.LogLevel;

namespace DotnetSpider.Core.Redial
{
	public class FileLockerRedialExecutor : RedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialLockerFile;
		private static readonly int RedialTimeout = 120 * 1000 / 50;

		private static readonly ConcurrentDictionary<string, Stream> Files = new ConcurrentDictionary<string, Stream>();

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
				catch (Exception e)
				{
					Logger.MyLog($"Delete atomic file failed: {e}", LogLevel.Error);
				}
			}
			try
			{
				File.Delete(RedialLockerFile);
			}
			catch (Exception e)
			{
				Logger.MyLog($"Delete redial lock file failed: {e}", LogLevel.Error);
			}
		}

		public override void WaitAll()
		{
			Files.TryAdd(RedialLockerFile, File.Create(RedialLockerFile));
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
			Files.TryAdd(id, File.Create(id));
			return id;
		}

		public override void DeleteActionIdentity(string identity)
		{
			Stream stream;
			Files.TryRemove(identity, out stream);
			stream?.Dispose();
			File.Delete(identity);
		}

		public override bool CheckIsRedialing()
		{
			lock (Lock)
			{
				if (!File.Exists(RedialLockerFile))
				{
					return false;
				}
				return true;
			}
		}

		public override void ReleaseRedialLocker()
		{
			Stream stream;
			Files.TryRemove(RedialLockerFile, out stream);
			stream?.Dispose();
			File.Delete(RedialLockerFile);
		}
	}
}
