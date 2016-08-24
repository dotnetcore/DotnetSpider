using System;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using Xunit;

namespace DotnetSpider.Test
{
	public class SpiderTest
	{
		[Fact]
		public void RunAsyncAndStop()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com");
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
			}
		}

		[Fact]
		public void WhenNoStartUrl()
		{
			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
			spider.Run();

			Assert.Equal(Status.Finished, spider.StatusCode);
		}

		private class TestPipeline : BasePipeline
		{
			public override void Process(ResultItems resultItems)
			{
				foreach (var entry in resultItems.Results)
				{
					Console.WriteLine($"{entry.Key}:{entry.Value}");
				}
			}
		}

		private class TestPageProcessor : IPageProcessor
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
	}
}
