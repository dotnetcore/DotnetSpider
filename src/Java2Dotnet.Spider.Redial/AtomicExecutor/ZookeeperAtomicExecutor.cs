#if !NET_CORE
using System;
using System.Linq;
using System.Threading;
using Java2Dotnet.Spider.Redial.RedialManager;
using Java2Dotnet.Spider.Redial.Utils;
using ZooKeeperNet;

namespace Java2Dotnet.Spider.Redial.AtomicExecutor
{
	internal class ZookeeperAtomicExecutor : IAtomicExecutor
	{
		private static readonly string _root = "/DotnetSpider";
		private static ZooKeeper _zk;

		public void Execute(string name, Action action)
		{
			WaitforRedial.WaitforRedialFinish();

			string id = GetPath(name);
			try
			{
				Zk.Create(id, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
				action();
			}
			finally
			{
				Zk.Delete(id, -1);
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			WaitforRedial.WaitforRedialFinish();

			string id = GetPath(name);

			try
			{
				Zk.Create(id, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
				action(obj);
			}
			finally
			{
				Zk.Delete(id, -1);
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			WaitforRedial.WaitforRedialFinish();

			string id = GetPath(name);

			Zk.Create(id, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
			try
			{
				return func(obj);
			}
			finally
			{
				Zk.Delete(id, -1);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			WaitforRedial.WaitforRedialFinish();
			string id = GetPath(name);

			try
			{
				Zk.Create(id, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
				return func();
			}
			finally
			{
				Zk.Delete(id, -1);
			}
		}

		public void WaitAtomicAction()
		{
			// 等待数据库等操作完成
			while (true)
			{
				if (Zk.GetChildren(_root, false).Count() == 1)
				{
					break;
				}

				Thread.Sleep(1);
			}
		}

		public IWaitforRedial WaitforRedial { get; }

		private static ZooKeeper Zk
		{
			get
			{
				if (_zk == null || Equals(_zk.State, ZooKeeper.States.CLOSED))
				{
					_zk = ZookeeperUtil.GetLongSessionZk();
				}
				return _zk;
			}
		}

		private string GetPath(string name)
		{
			return $"{_root}/{name + "-" + Guid.NewGuid().ToString("N")}";
		}

		internal ZookeeperAtomicExecutor(IWaitforRedial waitforRedial)
		{
			if (waitforRedial == null)
			{
				throw new RedialException("IWaitforRedial can't be null.");
			}
			WaitforRedial = waitforRedial;
			if (Zk.Exists(_root, false) == null)
			{
				Zk.Create(_root, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
			}
		}
	}
}
#endif