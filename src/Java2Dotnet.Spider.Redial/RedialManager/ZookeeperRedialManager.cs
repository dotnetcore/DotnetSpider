//#if !NET_CORE

//using System;
//using System.IO;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using Java2Dotnet.Spider.Redial.AtomicExecutor;
//using Java2Dotnet.Spider.Redial.Utils;
//using ZooKeeperNet;

//namespace Java2Dotnet.Spider.Redial.RedialManager
//{
//	/// <summary>
//	/// 用于单台电脑
//	/// </summary>
//	public class ZookeeperRedialManager : BaseRedialManager
//	{
//		private readonly string _lockerFilePath;
//		private static ZookeeperRedialManager _instanse;
//		private readonly int RedialTimeout = 60 * 1000 / 50;
//		private ZooKeeper _zk;
//		private string _root = "/DotnetSpider";

//		public override IAtomicExecutor AtomicExecutor { get; }

//		public static ZookeeperRedialManager Default
//		{
//			get
//			{
//				if (_instanse == null)
//				{
//					_instanse = new ZookeeperRedialManager();
//				}
//				return _instanse;
//			}
//		}

//		[MethodImpl(MethodImplOptions.Synchronized)]
//		public override void WaitforRedialFinish()
//		{
//			if (Skip)
//			{
//				return;
//			}

//			if (Zk.Exists(_lockerFilePath, false) != null)
//			{
//				for (int i = 0; i < RedialTimeout; ++i)
//				{
//					Thread.Sleep(50);
//					if (Zk.Exists(_lockerFilePath, false) == null)
//					{
//						break;
//					}
//				}
//			}
//		}

//		[MethodImpl(MethodImplOptions.Synchronized)]
//		public override RedialResult Redial()
//		{
//			if (Skip)
//			{
//				return RedialResult.Skip;
//			}

//			if (Zk.Exists(_lockerFilePath, false) != null)
//			{
//				while (true)
//				{
//					Thread.Sleep(50);
//					if (Zk.Exists(_lockerFilePath, false) == null)
//					{
//						return RedialResult.OtherRedialed;
//					}
//				}
//			}
//			else
//			{
//				try
//				{
//					using (var zk = ZookeeperUtil.GetShortSessionZk(120))
//					{
//						zk.Create(_lockerFilePath, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);

//						// wait all operation stop.
//						Thread.Sleep(5000);

//						Logger.Warn("Wait atomic action to finish...");

//						AtomicExecutor.WaitAtomicAction();

//						Logger.Warn("Try to redial network...");

//						RedialInternet();

//						Logger.Warn("Redial finished.");
//						return RedialResult.Sucess;
//					}
//				}
//				catch (IOException)
//				{
//					// 有极小可能同时调用File.Open时抛出异常
//					return Redial();
//				}
//				catch (Exception)
//				{
//					return RedialResult.Failed;
//				}
//			}
//		}

//		private ZookeeperRedialManager()
//		{
//			_lockerFilePath = $"{_root}/redialer.lock";

//			if (Zk.Exists(_root, false) == null)
//			{
//				Zk.Create(_root, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
//			}

//			AtomicExecutor = new ZookeeperAtomicExecutor(this);
//		}

//		private ZooKeeper Zk
//		{
//			get
//			{
//				if (_zk == null || Equals(_zk.State, ZooKeeper.States.CLOSED))
//				{
//					_zk = ZookeeperUtil.GetLongSessionZk();
//				}
//				return _zk;
//			}
//		}
//	}
//}
//#endif