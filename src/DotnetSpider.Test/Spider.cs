using System;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using Xunit;

namespace DotnetSpider.Test
{
	public class Spider
	{
		[Fact]
		public void RunAsyncAndStop()
		{
			Core.Spider spider = Core.Spider.Create(new Core.Site { EncodingName = "UTF-8", MinSleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline()).SetThreadNum(1);
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

		private class TestPipeline : BasePipeline
		{
			public override void Process(Core.ResultItems resultItems)
			{
				foreach (var entry in resultItems.Results)
				{
					Console.WriteLine($"{entry.Key}:{entry.Value}");
				}
			}
		}

		private class TestPageProcessor : IPageProcessor
		{
			public Core.Site Site
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
