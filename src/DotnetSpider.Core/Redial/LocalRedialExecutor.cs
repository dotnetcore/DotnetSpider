using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Redial.InternetDetector;
using System.Threading;

namespace DotnetSpider.Core.Redial
{
	public abstract class LocalRedialExecutor : RedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialTimeFile;
		private static readonly ConcurrentDictionary<string, Stream> NetworkFiles = new ConcurrentDictionary<string, Stream>();

		static LocalRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(Env.GlobalDirectory, "atomicaction");
			RedialTimeFile = Path.Combine(Env.GlobalDirectory, "redial.time");
		}

		public LocalRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
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

		public LocalRedialExecutor(IRedialer redialer) : this(redialer, new DefaultInternetDetector()) { }

		public LocalRedialExecutor() : this(new AdslRedialer(), new DefaultInternetDetector())
		{
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

		protected override DateTime GetLastRedialTime()
		{
			if (File.Exists(RedialTimeFile))
			{
				long ticks;
				if (long.TryParse(File.ReadAllText(RedialTimeFile), out ticks))
				{
					return new DateTime(ticks);
				}
				else
				{
					return DateTime.Now.AddDays(-1);
				}
			}
			else
			{
				return DateTime.Now.AddDays(-1);
			}
		}

		protected override void RecordLastRedialTime()
		{
			File.WriteAllText(RedialTimeFile, DateTime.Now.Ticks.ToString());
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
	}
}
