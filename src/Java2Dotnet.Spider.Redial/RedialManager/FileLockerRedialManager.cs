using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.Redialer;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于单台电脑
	/// </summary>
	public class FileLockerRedialManager : BaseRedialManager
	{
		private readonly string _lockerFilePath;
		private readonly int RedialTimeout = 120 * 1000 / 50;

		public override IAtomicExecutor AtomicExecutor { get; }

		public FileLockerRedialManager(INetworkValidater networkValidater, IRedialer redialer) : base()
		{
			_lockerFilePath = Path.Combine(SpiderEnviroment.GlobalDirectory, "redialer.lock");
			AtomicExecutor = new FileLockerAtomicExecutor(this);
			NetworkValidater = networkValidater;
			Redialer = redialer;
		}

		public override void WaitforRedialFinish()
		{
			lock (this)
			{
				if (Skip)
				{
					return;
				}

				if (File.Exists(_lockerFilePath))
				{
					for (int i = 0; i < RedialTimeout; ++i)
					{
						Thread.Sleep(50);
						if (!File.Exists(_lockerFilePath))
						{
							break;
						}
					}
				}
			}
		}

		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

			if (File.Exists(_lockerFilePath))
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!File.Exists(_lockerFilePath))
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				Stream stream = null;

				try
				{
					stream = File.Open(_lockerFilePath, FileMode.Create, FileAccess.Write);

					// wait all operation stop.
					Thread.Sleep(5000);

					Logger.Warn("Wait atomic action to finish...");

					// 等待数据库等操作完成
					AtomicExecutor.WaitAtomicAction();

					Logger.Warn("Try to redial network...");

					RedialInternet();

					Logger.Warn("Redial finished.");
					return RedialResult.Sucess;
				}
				catch (IOException)
				{
					// 有极小可能同时调用File.Open时抛出异常
					return Redial();
				}
				catch (Exception)
				{
					return RedialResult.Failed;
				}
				finally
				{

#if !NET_CORE
					stream?.Close();
#else
					stream?.Dispose();
#endif
					File.Delete(_lockerFilePath);
				}
			}
		}
	}
}
