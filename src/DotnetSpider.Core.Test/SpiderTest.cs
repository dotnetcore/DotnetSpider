using System;
using System.Threading;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using Xunit;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Downloader;
using DotnetSpider.Core.Scheduler;

namespace DotnetSpider.Core.Test
{
	internal class CountResult
	{
		public int Count { get; set; }
	}


	public partial class SpiderTest
	{
		[Fact(DisplayName = "Spider_IdentityLengthLimit")]
		public void IdentityLengthLimit()
		{
			try
			{
				Spider.Create(
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			}
			catch (Exception exception)
			{
				Assert.Equal($"Length of identity should less than {Env.IdentityMaxLength}.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}

		[Fact(DisplayName = "DefaultConstruct")]
		public void DefaultConstruct()
		{
			Spider spider = new Spider();
		}

		[Fact(DisplayName = "RunAsyncAndStop")]
		public void RunAsyncAndStop()
		{
			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
			{
				return;
			}
			Spider spider = Spider.Create(new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.SleepTime = 1000;
			for (int i = 0; i < 10000; i++)
			{
				spider.AddRequest(new Request("http://www.baidu.com/" + i) { EncodingName = "UTF-8" });
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.RunAsync();
			});
			Thread.Sleep(3000);
		}

		[Fact(DisplayName = "RunAsyncAndContiune")]
		public void RunAsyncAndContiune()
		{
			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
			{
				return;
			}
			Spider spider = Spider.Create(new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.EncodingName = "UTF-8";
			for (int i = 0; i < 10000; i++)
			{
				spider.AddRequests("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.Contiune();
			});
			Thread.Sleep(5000);
		}

		[Fact(DisplayName = "RunAsyncAndStopThenExit")]
		public void RunAsyncAndStopThenExit()
		{
			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
			{
				return;
			}
			Spider spider = Spider.Create(new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.EncodingName = "UTF-8";
			spider.SleepTime = 1000;
			for (int i = 0; i < 10000; i++)
			{
				spider.AddRequests("http://www.baidu.com/" + i);
			}
			spider.RunAsync();
			Thread.Sleep(5000);
			spider.Pause(() =>
			{
				spider.Exit();
			});
			Thread.Sleep(5000);
		}

		[Fact(DisplayName = "NoPipeline")]
		public void NoPipeline()
		{
			Spider spider = Spider.Create(new TestPageProcessor());
			spider.EmptySleepTime = 1000;
			spider.EncodingName = "UTF-8";
			spider.SleepTime = 1000;
			spider.Run();
		}

		[Fact(DisplayName = "WhenNoStartUrl")]
		public void WhenNoStartUrl()
		{
			Spider spider = Spider.Create(new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.EncodingName = "UTF-8";
			spider.SleepTime = 1000;
			spider.Run();

			Assert.Equal(Status.Finished, spider.Status);
		}

		internal class TestPipeline : BasePipeline
		{
			public override void Process(IList<ResultItems> resultItems, dynamic sender = null)
			{
				foreach (var resultItem in resultItems)
				{
					foreach (var entry in resultItem)
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
				page.Bypass = true;
			}
		}

		[Fact(DisplayName = "RetryWhenResultIsEmpty")]
		public void RetryWhenResultIsEmpty()
		{
			Spider spider = Spider.Create(new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			spider.EncodingName = "UTF-8";
			spider.CycleRetryTimes = 5;
			spider.SleepTime = 1000;
			spider.AddRequests("http://taobao.com");
			spider.Run();

			Assert.Equal(Status.Finished, spider.Status);
		}

		[Fact(DisplayName = "CloseSignal")]
		public void CloseSignal()
		{
			Spider spider = Spider.Create(
				new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.EncodingName = "UTF-8";
			spider.CycleRetryTimes = 5;
			spider.ClearSchedulerAfterCompleted = false;
			for (int i = 0; i < 20; ++i)
			{
				spider.AddRequests($"http://www.baidu.com/t={i}");
			}
			var task = spider.RunAsync();
			Thread.Sleep(500);
			spider.SendExitSignal();
			task.Wait();
			Assert.Equal(10, spider.Scheduler.SuccessRequestsCount);

			Spider spider2 = Spider.Create(
				new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider2.ClearSchedulerAfterCompleted = false;
			spider2.EncodingName = "UTF-8";
			spider2.CycleRetryTimes = 5;
			for (int i = 0; i < 25; ++i)
			{
				spider2.AddRequests($"http://www.baidu.com/t={i}");
			}
			spider2.Run();
			Assert.Equal(25, spider2.Scheduler.SuccessRequestsCount);
		}

		[Fact(DisplayName = "FastExit")]
		public void FastExit()
		{
			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
			{
				return;
			}
			var path = "FastExit_Result.txt";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Spider spider = Spider.Create(
				new FastExitPageProcessor())
			.AddPipeline(new FastExitPipeline());
			spider.ThreadNum = 1;
			spider.EmptySleepTime = 0;
			spider.EncodingName = "UTF-8";
			spider.CycleRetryTimes = 5;
			spider.SleepTime = 0;
			spider.AddRequests("http://war.163.com/");
			spider.AddRequests("http://sports.163.com/");
			spider.AddRequests("http://ent.163.com/");
			spider.Downloader = new TestDownloader();
			spider.Run();
			stopwatch.Stop();
			var costTime = stopwatch.ElapsedMilliseconds;
			Assert.True(costTime < 3000);
			var results = File.ReadAllLines("FastExit_Result.txt");
			Assert.Contains("http://war.163.com/", results);
			Assert.Contains("http://sports.163.com/", results);
			Assert.Contains("http://ent.163.com/", results);
		}

		internal class TestDownloader : DotnetSpider.Downloader.Downloader
		{
			protected override Response DowloadContent(Request request)
			{
				return new Response() { Request = request, Content = "aabbcccdefg下载人数100", TargetUrl = request.Url };
			}
		}

		internal class FastExitPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				page.AddResultItem("a", "b");
			}
		}

		internal class FileDownloader : DotnetSpider.Downloader.Downloader
		{
			protected override Response DowloadContent(Request request)
			{
				return new Response { Request = request };
			}
		}

		internal class FastExitPipeline : BasePipeline
		{
			public override void Process(IList<ResultItems> resultItems, dynamic sender = null)
			{
				File.AppendAllLines("FastExit_Result.txt", new[] { resultItems.First().Request.Url.ToString() });
			}
		}

		//[Fact]
		//public void TestReturnHttpProxy()
		//{
		//	Spider spider = Spider.Create(new Site { HttpProxyPool = new HttpProxyPool(new KuaidailiProxySupplier("代理链接")), EncodingName = "UTF-8", MinSleepTime = 1000, Timeout = 20000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
		//	for (int i = 0; i < 500; i++)
		//	{
		//		spider.AddRequest("http://www.taobao.com/" + i);
		//	}
		//	spider.Run();

		//	Assert.Equal(Status.Finished, spider.StatusCode);
		//}
	}
}
