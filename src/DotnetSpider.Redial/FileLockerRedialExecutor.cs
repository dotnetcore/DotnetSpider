using System;
using System.IO;
using System.Threading;
using DotnetSpider.Redial.Redialer;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Redial.InternetDetector;

namespace DotnetSpider.Redial
{
	public class FileLockerRedialExecutor : RedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialLockerFile;
		private static readonly int RedialTimeout = 120 * 1000 / 50;

		static FileLockerRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(SpiderConsts.GlobalDirectory, "atomicaction");
			RedialLockerFile = Path.Combine(SpiderConsts.GlobalDirectory, "redial.lock");
		}

		public FileLockerRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
		{
		}

		public override void WaitAll()
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
			File.Create(id).Dispose();
			return id;
		}

		public override void DeleteActionIdentity(string identity)
		{
			File.Delete(identity);
		}

		public override bool CheckIsRedialing()
		{
			if (File.Exists(RedialLockerFile))
			{
				File.Create(RedialLockerFile).Dispose();
				return false;
			}
			return true;
		}

		public override void ReleaseRedialLocker()
		{
			File.Delete(RedialLockerFile);
		}

		public override void Init()
		{
			if (!Directory.Exists(AtomicActionFolder))
			{
				Directory.CreateDirectory(AtomicActionFolder);
			}
		}
	}
}
