using System;
using System.IO;
using System.Threading;
using DotnetSpider.Core.Common;
using DotnetSpider.Redial.AtomicExecutor;
using DotnetSpider.Redial.NetworkValidater;
using DotnetSpider.Redial.Redialer;

namespace DotnetSpider.Redial.RedialManager
{
	/// <summary>
	/// 用于单台电脑
	/// </summary>
	public class FileLockerRedialManager : BaseRedialManager
	{
		private readonly string _lockerFilePath;
		private readonly int _redialTimeout = 120 * 1000 / 50;

		public override IAtomicExecutor AtomicExecutor { get; }

		public FileLockerRedialManager(INetworkValidater networkValidater, IRedialer redialer)
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
					for (int i = 0; i < _redialTimeout; ++i)
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

					//Logger.Log("Wait atomic action to finish...");

					// 等待数据库等操作完成
					AtomicExecutor.WaitAtomicAction();

					//Logger.Log("Try to redial network...");

					RedialInternet();

					//Logger.Log("Redial finished.");
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
