using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core.Redial.InternetDetector;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Scheduler;
using Xunit;
using System;

namespace DotnetSpider.Core.Test.Redial
{

	public class RedialTest
	{
		[Fact]
		public void Setting()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };

			// Set start/seed url
			site.AddStartUrl("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				new QueueDuplicateRemovedScheduler(),
				// default page processor will save whole html, and extract urls to target urls via regex
				new DefaultPageProcessor(new[] { "cnblogs\\.com" }))
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());

			spider.RedialExecutor = new FileLockerRedialExecutor(new AdslRedialer("", "", ""), new VpsInternetDetector());
			Assert.NotNull(spider.RedialExecutor);
		}
	}
}
