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
	public class MutexRedialExecutor : RedialExecutor
	{
		private const string MutexName = "DotnetSpiderRedialLocker";
		private static readonly string AtomicActionFolder;
		private static readonly string RedialTimeFile;
		private static Mutex SyncNamed;
		private static readonly ConcurrentDictionary<string, Stream> NetworkFiles = new ConcurrentDictionary<string, Stream>();

		static MutexRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(Env.GlobalDirectory, "atomicaction");
			RedialTimeFile = Path.Combine(Env.GlobalDirectory, "redial.time");
		}

		public MutexRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
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
		}

		public override void WaitAllNetworkRequestComplete()
		{
			while (true)
			{
				if (!Directory.GetFiles(AtomicActionFolder).Any())
				{
					break;
				}
				Thread.Sleep(50);
			}
		}

		public override bool IsRedialing()
		{
			try
			{
				Mutex.OpenExisting(MutexName).Dispose();
				return true;
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				return false;
			}
		}

		public override string CreateActionIdentity(string name)
		{
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			NetworkFiles.TryAdd(id, File.Create(id));
			return id;
		}

		public override void DeleteActionIdentity(string identity)
		{
			NetworkFiles.TryRemove(identity, out var stream);
			stream?.Dispose();
			File.Delete(identity);
		}

		public override void LockRedial()
		{
			try
			{
				SyncNamed = Mutex.OpenExisting(MutexName);       //如果此命名互斥对象已存在则请求打开
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				SyncNamed = new Mutex(false, MutexName);
			}
			SyncNamed.WaitOne();
		}

		public override void ReleaseRedialLock()
		{
			SyncNamed.ReleaseMutex();
			SyncNamed.Dispose();
		}

		public override DateTime GetLastRedialTime()
		{
			if (File.Exists(RedialTimeFile))
			{
				return DateTime.Parse(File.ReadAllText(RedialTimeFile).Trim());
			}
			else
			{
				return new DateTime();
			}
		}

		public override void RecordLastRedialTime()
		{
			File.AppendAllText(RedialTimeFile, DateTime.Now.ToString());
		}
	}
}
