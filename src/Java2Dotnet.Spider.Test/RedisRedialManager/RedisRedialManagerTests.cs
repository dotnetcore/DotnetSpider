#if !NET_CORE
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.Redialer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
namespace Java2Dotnet.Spider.Test.RedisRedialManager
{
	[TestClass]
	public class RedisRedialManagerTests
	{
		[TestMethod]
		public void AtomicCommonTest()
		{
			var manager = Redial.RedialManager.RedisRedialManager.Create("localhost");
			manager.Redis.GetServer(manager.Redis.GetEndPoints()[0]).FlushDatabase(1);

			RedisAtomicExecutor executor = new RedisAtomicExecutor(manager);
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Thread.Sleep(500);
			var result = manager.Db.HashGetAll(RedisAtomicExecutor.GetSetKey());
			Assert.IsTrue(result.Length == 1);
			Assert.IsTrue(result[0].ToString().StartsWith("test"));
			Thread.Sleep(5 * 1000);
			result = manager.Db.HashGetAll(RedisAtomicExecutor.GetSetKey());
			Assert.IsTrue(result.Length == 0);
		}

		[TestMethod]
		public void RedialTest()
		{
			var manager = Redial.RedialManager.RedisRedialManager.Create("localhost");
			manager.Redis.GetServer(manager.Redis.GetEndPoints()[0]).FlushDatabase(1);
			manager.Redialer = new TestRedial();
			RedisAtomicExecutor executor = new RedisAtomicExecutor(manager);
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test1", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test2", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test3", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			DateTime time1 = DateTime.Now;
			manager.Redial();
			DateTime time2 = DateTime.Now;
			Assert.IsTrue((time2 - time1).Seconds > 4);
		}

		[TestMethod]
		public void ClearTimeoutTest1()
		{
			var manager = Redial.RedialManager.RedisRedialManager.Create("localhost");
			manager.Redis.GetServer(manager.Redis.GetEndPoints()[0]).FlushDatabase(1);
			manager.Redialer = new TestRedial();
			manager.Db.HashSet(RedisAtomicExecutor.GetSetKey(), "test-timeout", DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd hh:mm"));
			RedisAtomicExecutor executor = new RedisAtomicExecutor(manager);
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test1", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test2", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test3", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			DateTime time1 = DateTime.Now;
			manager.Redial();
			DateTime time2 = DateTime.Now;
			Assert.IsTrue((time2 - time1).Seconds > 4);
		}

		[TestMethod]
		public void ClearTimeoutTest2()
		{
			var manager = Redial.RedialManager.RedisRedialManager.Create("localhost");
			manager.Redis.GetServer(manager.Redis.GetEndPoints()[0]).FlushDatabase(1);
			manager.Redialer = new TestRedial();
			manager.Db.HashSet(RedisAtomicExecutor.GetSetKey(), Redial.RedialManager.RedisRedialManager.Locker, DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd hh:mm"));
			RedisAtomicExecutor executor = new RedisAtomicExecutor(manager);
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test1", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test2", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			Task.Factory.StartNew(() =>
			{
				executor.Execute("test3", () =>
				{
					for (int i = 0; i < 5; ++i)
					{
						Thread.Sleep(1000);
					}
				});
			});
			DateTime time1 = DateTime.Now;
			manager.Redial();
			DateTime time2 = DateTime.Now;
			Assert.IsTrue((time2 - time1).Seconds > 4);
		}

		public class TestRedial : BaseAdslRedialer
		{
			public override void Redial()
			{
				Console.WriteLine("Redial Success.");
			}
		}
	}
}
