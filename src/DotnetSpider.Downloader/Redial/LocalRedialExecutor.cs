using DotnetSpider.Downloader.Redial.InternetDetector;
using DotnetSpider.Downloader.Redial.Redialer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DotnetSpider.Downloader.Redial
{
	/// <summary>
	/// 单机拨号器+网络通讯器
	/// </summary>
	public abstract class LocalRedialExecutor : RedialExecutor
	{
		private static readonly string AtomicActionFolder;
		private static readonly string RedialTimeFile;
		private static readonly Dictionary<string, Stream> NetworkFiles = new Dictionary<string, Stream>();
		private static readonly object NetworkFilesLocker = new object();

		static LocalRedialExecutor()
		{
			AtomicActionFolder = Path.Combine(DownloaderEnv.GlobalDirectory, "atomicaction");
			RedialTimeFile = Path.Combine(DownloaderEnv.GlobalDirectory, "redial.time");
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="redialer">拨号器</param>
		/// <param name="validater">网络状态检测器</param>
		protected LocalRedialExecutor(IRedialer redialer, IInternetDetector validater) : base(redialer, validater)
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
					// ignored
				}
			}
		}

		/// <summary>
		/// 创建通讯标识
		/// </summary>
		/// <param name="name">通讯标识前缀</param>
		/// <returns>通讯标识</returns>
		public override string CreateActionIdentity(string name)
		{
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			lock (NetworkFilesLocker)
			{
				NetworkFiles.Add(id, File.Create(id));
			}
			return id;
		}

		/// <summary>
		/// 删除通讯标识
		/// </summary>
		/// <param name="identity">通讯标识</param>
		public override void DeleteActionIdentity(string identity)
		{
			lock (NetworkFilesLocker)
			{
				if (NetworkFiles.ContainsKey(identity))
				{
					lock (NetworkFilesLocker)
					{
						var stream = NetworkFiles[identity];
						NetworkFiles.Remove(identity);
						stream?.Dispose();
						File.Delete(identity);
					}
				}
			}
		}

		/// <summary>
		/// 取得上次拨号的时间, 如果间隔太短则不执行拨号操作
		/// </summary>
		/// <returns>上次拨号的时间</returns>
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

		/// <summary>
		/// 记录拨号时间
		/// </summary>
		protected override void RecordLastRedialTime()
		{
			File.WriteAllText(RedialTimeFile, DateTime.Now.Ticks.ToString());
		}

		/// <summary>
		/// 等待所有网络通讯结束
		/// </summary>
		public override void WaitAllNetworkRequestComplete()
		{
			while (true)
			{
				if (Directory.GetFiles(AtomicActionFolder).Length == 0)
				{
					break;
				}
				Thread.Sleep(50);
			}
		}
	}
}
