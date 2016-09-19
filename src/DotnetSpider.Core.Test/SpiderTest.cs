using System;
using System.Threading;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using Xunit;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Scheduler;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Proxy;
using DotnetSpider.Core.Selector;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Core.Test
{
	public class CountResult
	{
		public int Count { get; set; }
	}

	public class SpiderTest
	{
		[Fact]
		public void UserIdAndTaskGroupAndIdentityLengthLimit()
		{
			try
			{
				Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 },
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new TestPageProcessor(), new QueueDuplicateRemovedScheduler());
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Length of Identity should less than 100.", exception.Message);
			}

			try
			{
				Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 },
					"abc",
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new TestPageProcessor(), new QueueDuplicateRemovedScheduler());
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Length of UserId should less than 100.", exception.Message);
			}

			try
			{
				Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 },
					"abcd",
					"abbb",
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new TestPageProcessor(), new QueueDuplicateRemovedScheduler());
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Length of TaskGroup should less than 100.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}

		[Fact]
		public void RunAsyncAndStop()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Stop();
			Thread.Sleep(5000);
			spider.RunAsync();
			Thread.Sleep(5000);
		}

		[Fact]
		public void ThrowExceptionWhenNoPipeline()
		{
			try
			{
				Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor());
				spider.Run();
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Pipelines should not be null.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}

		[Fact]
		public void DatebaseLogAndStatus()
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
			string id = Guid.NewGuid().ToString("N");
			string taskGroup = Guid.NewGuid().ToString("N");
			string userId = Guid.NewGuid().ToString("N");
			string connectString = "Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306";
			Configuration.SetValue("logAndStatusConnectString", connectString);
			Assert.Equal("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306", Configuration.GetValue("logAndStatusConnectString"));
			LogManagerHelper.InitLogManager(true);
			using (Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 },
				id,
				userId,
				taskGroup,
				new TestPageProcessor(), new QueueDuplicateRemovedScheduler()))
			{
				spider.AddPipeline(new TestPipeline()).SetThreadNum(1);
				for (int i = 0; i < 5; i++)
				{
					spider.AddStartUrl("http://www.baidu.com/" + i);
				}
				SpiderMonitor.Register(spider);
				spider.Run();
			}
			using (MySqlConnection conn = new MySqlConnection(connectString))
			{
				Assert.Equal(1, conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.status where userid='{userId}' and taskgroup='{taskGroup}' and identity='{id}'").First().Count);
				Assert.Equal(7, conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.log where userid='{userId}' and taskgroup='{taskGroup}' and identity='{id}'").First().Count);
			}
		}

		[Fact]
		public void WhenNoStartUrl()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			spider.Run();

			Assert.Equal(Status.Finished, spider.StatusCode);
		}

		public class TestPipeline : BasePipeline
		{
			public override void Process(ResultItems resultItems)
			{
				foreach (var entry in resultItems.Results)
				{
					Console.WriteLine($"{entry.Key}:{entry.Value}");
				}
			}
		}

		public class TestPageProcessor : IPageProcessor
		{
			public Site Site
			{
				get; set;
			}

			public void Process(Page page)
			{
				page.IsSkip = true;
			}
		}

		[Fact]
		public void TestRetryWhenResultIsEmpty()
		{
			Spider spider = Spider.Create(new Site { CycleRetryTimes = 5, EncodingName = "UTF-8", MinSleepTime = 1000, Timeout = 20000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			spider.AddStartUrl("http://taobao.com");
			spider.RetryWhenResultIsEmpty = true;
			spider.Run();

			Assert.Equal(Status.Finished, spider.StatusCode);
		}

		//[Fact]
		//public void TestReturnHttpProxy()
		//{
		//	Spider spider = Spider.Create(new Site { HttpProxyPool = new HttpProxyPool(new KuaidailiProxySupplier("代理链接")), EncodingName = "UTF-8", MinSleepTime = 1000, Timeout = 20000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
		//	for (int i = 0; i < 500; i++)
		//	{
		//		spider.AddStartUrl("http://www.taobao.com/" + i);
		//	}
		//	spider.Run();

		//	Assert.Equal(Status.Finished, spider.StatusCode);
		//}
	}
}
