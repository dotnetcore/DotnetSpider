using System;
using System.Threading;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using Xunit;

namespace DotnetSpider.Core.Test
{
	internal class CountResult
	{
		public int Count { get; set; }
	}

	
	public class SpiderTest
	{
		[Fact]
		public void IdentityLengthLimit()
		{
			try
			{
				Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			}
			catch (Exception exception)
			{
				Assert.Equal("Length of Identity should less than 100.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}

		[Fact]
		public void RunAsyncAndStop()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.RunAsync();
			});
			Thread.Sleep(3000);
		}

		[Fact]
		public void RunAsyncAndContiune()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.Contiune();
			});
			Thread.Sleep(5000);
		}

		[Fact]
		public void RunAsyncAndStopThenExit()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.Exit();
			});
			Thread.Sleep(5000);
		}

		[Fact]
		public void ThrowExceptionWhenNoPipeline()
		{
			try
			{
				Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor());
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
		public void WhenNoStartUrl()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.Run();

			Assert.Equal(Status.Finished, spider.Stat);
		}

		internal class TestPipeline : BasePipeline
		{
			public override void Process(params ResultItems[] resultItems)
			{
				foreach (var resultItem in resultItems)
				{
					foreach (var entry in resultItem.Results)
					{
						Console.WriteLine($"{entry.Key}:{entry.Value}");
					}
				}
			}
		}

		internal class TestPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				page.Skip = true;
			}
		}

		[Fact]
		public void TestRetryWhenResultIsEmpty()
		{
			Spider spider = Spider.Create(new Site { CycleRetryTimes = 5, EncodingName = "UTF-8", SleepTime = 1000, Timeout = 20000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.AddStartUrl("http://taobao.com");
			spider.Run();

			Assert.Equal(Status.Finished, spider.Stat);
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
