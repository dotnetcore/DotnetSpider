using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace RedisSharp
{

	using System.Text;
	using System;
	using System.Collections.Generic;

	[TestClass]
	public class RedisTest
	{

		private static int nPassed = 0;
		private static int nFailed = 0;

		[TestMethod]
		public void RedisBaseTest()
		{
			RedisServer r = new RedisServer("localhost");

			r.Db = 3;

			#region set

			long i;
			r.SetLength("a");
			r.SetAdd("foo", "bar");
			r.FlushDb();

			Assert.IsTrue((i = r.SetLength("foo")) == 0, "there should be no keys but there were {0}", i);
			r.SetAdd("foo", "bar");
			Assert.IsTrue((i = r.SetLength("foo")) == 1, "there should be one key but there were {0}", i);
			r.SetAdd("foo bär", "bär foo");
			r.SetAdd("foo", "bär foo");
			Assert.IsTrue((i = r.SetLength("foo")) == 2, "there should be two keys but there were {0}", i);

			Assert.IsTrue(r.TypeOf("foo") == KeyType.Set, "type is not string");
			r.SetAdd("bar", "foo");

			Assert.IsTrue(r.SetContains("bar", "foo"));

			var mems = r.SetMembers("foo");
			Assert.AreEqual("bar", mems[1]);
			Assert.AreEqual("bär foo", mems[0]);

			r.SetRemove("foo", "bar");
			mems = r.SetMembers("foo");
			Assert.AreEqual("bär foo", mems[0]);
			Assert.AreEqual(1, mems.Count);

			string item = r.SetPop("foo");
			Assert.AreEqual("bär foo", item);
			Assert.AreEqual(0, r.SetLength("foo"));

			#endregion

			#region hasset

			r.HashSet("set", "a", "a");
			r.HashSet("set", "b", "b");
			r.HashSet("set", "c", "c");
			var hash = r.HashGetAll("set");
			Assert.AreEqual(hash.Length, 3);

			r.HashDelete("set", "a");
			hash = r.HashGetAll("set");
			Assert.AreEqual(hash.Length, 2);

			Assert.AreEqual(true, r.HashExists("set", "b"));
			Assert.AreEqual("b", r.HashGet("set", "b"));
			Assert.AreEqual(2, r.HashLength("set"));

			#endregion

			#region storedset

			r.SortedSetAdd("sortedset", "a", 0L);
			r.SortedSetAdd("sortedset", "b", 0L);
			r.SortedSetAdd("sortedset", "c", 0L);
			r.SortedSetAdd("sortedset", "d", 0L);
			Assert.AreEqual(4, r.SortedSetLength("sortedset"));
			r.SortedSetRemove("sortedset", "a");
			Assert.AreEqual(3, r.SortedSetLength("sortedset"));

			string[] sr = r.SortedSetRangeByRank("sortedset", 0, 2);
			Assert.AreEqual("b", sr[0]);
			Assert.AreEqual("c", sr[1]);
			Assert.AreEqual("d", sr[2]);

			#endregion


			#region list

			r.ListLeftPush("list", "1");
			r.ListLeftPush("list", "2");
			r.ListLeftPush("list", "3");
			r.ListLeftPush("list", "4");
			r.ListRightPush("list", "5");
			Assert.AreEqual(5, r.ListLength("list"));
			Assert.AreEqual("4", r.ListLeftPop("list"));
			Assert.AreEqual("5", r.ListRightPop("list"));
			Assert.AreEqual("1", r.ListRightPop("list"));
			Assert.AreEqual("3", r.ListLeftPop("list"));

			#endregion

			r.FlushDb();
			r.Dispose();
		}

		[TestMethod]
		public void MultiTheadTestForStactExtanceRedis()
		{
			var context = ConnectionMultiplexer.Connect(new ConfigurationOptions()
			{
				ServiceName = "localhost",
				ConnectTimeout = 5000,
				KeepAlive = 8,
				AllowAdmin = true,
				EndPoints =
				{
					{"localhost", 6379}
				}
			});
			var r = context.GetDatabase(3);
 
			DateTime start1 = DateTime.Now;

			Parallel.For(0, 100000, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, j =>
			{
				int i = j;
				r.ListLeftPush("list", "1" + i);
				r.ListLeftPush("list", "2" + i);
				r.ListLeftPush("list", "3" + i);
				r.ListLeftPush("list", "4" + i);
				r.ListRightPush("list", "5" + i);
				r.ListLeftPop("list");
				r.ListRightPop("list");
			});

			DateTime end1 = DateTime.Now;
			double seconds1 = (end1 - start1).TotalSeconds;

			DateTime start2 = DateTime.Now;

			for (int i = 0; i < 100000; ++i)
			{
				r.ListLeftPush("list", "1" + i);
				r.ListLeftPush("list", "2" + i);
				r.ListLeftPush("list", "3" + i);
				r.ListLeftPush("list", "4" + i);
				r.ListRightPush("list", "5" + i);
				r.ListLeftPop("list");
				r.ListRightPop("list");
			}
			DateTime end2 = DateTime.Now;
			double seconds2 = (end2 - start2).TotalSeconds;
		}

		[TestMethod]
		public void MultiTheadTest()
		{
			RedisServer r = new RedisServer("localhost", 6379, null, 100);
			r.Db = 3;

			DateTime start1 = DateTime.Now;

			Parallel.For(0, 100000, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, j =>
			{
				int i = j;
				r.ListLeftPush("list", "1" + i);
				r.ListLeftPush("list", "2" + i);
				r.ListLeftPush("list", "3" + i);
				r.ListLeftPush("list", "4" + i);
				r.ListRightPush("list", "5" + i);
				r.ListLeftPop("list");
				r.ListRightPop("list");
			});

			DateTime end1 = DateTime.Now;
			double seconds1 = (end1 - start1).TotalSeconds;

			DateTime start2 = DateTime.Now;

			for (int i = 0; i < 100000; ++i)
			{
				r.ListLeftPush("list", "1" + i);
				r.ListLeftPush("list", "2" + i);
				r.ListLeftPush("list", "3" + i);
				r.ListLeftPush("list", "4" + i);
				r.ListRightPush("list", "5" + i);
				r.ListLeftPop("list");
				r.ListRightPop("list");
			}
			DateTime end2 = DateTime.Now;
			double seconds2 = (end2 - start2).TotalSeconds;
		}
	}
}
