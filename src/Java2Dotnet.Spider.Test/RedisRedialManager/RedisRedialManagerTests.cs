#if !NET_CORE
using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.Redialer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisSharp;


namespace Java2Dotnet.Spider.Test.RedisRedialManager
{
	[TestClass]
	public class RedisRedialManagerTests
	{
		[TestMethod]
		public void SubPubTest()
		{
			var redis = new RedisServer();
			//redis.PreserveAsyncOrder = false;

			string result = "";
			redis.Subscribe("messages", (channel, message) =>
			{
				result = message;
			});

			redis.Publish("messages", "hello");

			Thread.Sleep(2000);

			Assert.AreEqual("hello", result);
			redis.Dispose();
		}

		[TestMethod]
		public void AtomicCommonTest()
		{
			Redial.RedialManager.RedisRedialManager manager = new Redial.RedialManager.RedisRedialManager("localhost", "", null);
			manager.Redis.FlushDb();
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
			var result = manager.Redis.HashGetAll(RedisAtomicExecutor.GetSetKey());
			Assert.IsTrue(result.Length == 1);
			Assert.IsTrue(result[0].Key.StartsWith("test"));
			Thread.Sleep(5 * 1000);
			result = manager.Redis.HashGetAll(RedisAtomicExecutor.GetSetKey());
			Assert.IsTrue(result.Length == 0);
		}

		[TestMethod]
		public void RedialTest()
		{
			var manager = new Redial.RedialManager.RedisRedialManager("localhost", null, null);
			manager.Redis.FlushDb();
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
			var manager = new Redial.RedialManager.RedisRedialManager("localhost", null, null);
			manager.Redis.FlushDb();
			manager.Redialer = new TestRedial();
			manager.Redis.HashSet(RedisAtomicExecutor.GetSetKey(), "test-timeout", DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm"));
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
			var manager = new Redial.RedialManager.RedisRedialManager("localhost", null, null);
			manager.Redis.FlushDb();
			manager.Redialer = new TestRedial();
			manager.Redis.HashSet(RedisAtomicExecutor.GetSetKey(), Redial.RedialManager.RedisRedialManager.Locker, DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm"));

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
#endif