using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DotnetSpider.Core.Test.Processor
{
	[TestClass]
	public class ProcessorTest
	{
		[TestMethod]
		public void ProcessException()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };

			var scheduler = new QueueDuplicateRemovedScheduler();

			site.AddStartUrl("http://v.youku.com/v_show/id_XMTMyMTkzNTY1Mg==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjkzNzMwMDMyOA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjcwNDg0NDI3Mg==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMTMwNzQwMTcwMA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjk1MzI0Mzk4NA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://v.youku.com/v_show/id_XMjkzNzY0NzkyOA==.html?spm=a2h1n.8251845.0.0");
			site.AddStartUrl("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				scheduler,
				// default page processor will save whole html, and extract urls to target urls via regex
				new TestPageProcessor())
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());

			// dowload html by http client
			spider.Downloader = new HttpClientDownloader();

			spider.ThreadNum = 1;
			// traversal deep 遍历深度
			spider.Deep = 3;

			// start crawler 启动爬虫
			spider.Run();

			Assert.AreEqual(4, spider.RetriedTimes.Value);
			Assert.AreEqual(0, scheduler.GetLeftRequestsCount());
			Assert.AreEqual(6, scheduler.GetSuccessRequestsCount());
			Assert.AreEqual(5, scheduler.GetErrorRequestsCount());
		}

		class TestPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				if (page.Request.Url.ToString() == "http://www.cnblogs.com/")
				{
					throw new SpiderException("");
				}
			}
		}
	}
}
