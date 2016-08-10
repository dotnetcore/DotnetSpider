using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Common;
using DotnetSpider.Redial.Redialer;
using System.Linq;
using DotnetSpider.Redial.InternetDetector;

namespace DotnetSpider.Redial
{
	public class FileLockerRedialExecutor : BaseRedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialLockerFile;
		private static readonly int RedialTimeout = 120 * 1000 / 50;

		static FileLockerRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(SpiderEnviroment.GlobalDirectory, "atomicaction");
			RedialLockerFile = Path.Combine(SpiderEnviroment.GlobalDirectory, "redial.lock");
		}

		public FileLockerRedialExecutor(IRedialer redialer,IInternetDetector validater) : base(redialer, validater)
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
			File.Create(id);
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
				File.Create(RedialLockerFile);
				return false;
			}
			return true;
		}

		public override void ReleaseRedialLocker()
		{
			File.Delete(RedialLockerFile);
		}
	}
}
