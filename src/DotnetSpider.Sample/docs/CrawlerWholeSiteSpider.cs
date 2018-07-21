using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.docs
{
	public class CrawlerWholeSiteSpider
	{
		public static void Run()
		{
			// Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8" };

			// Set start/seed url
			site.AddRequests("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,
				// use memoery queue scheduler
				new QueueDuplicateRemovedScheduler(),
				// default page processor will save whole html, and extract urls to target urls via regex
				new DefaultPageProcessor(new[] { "cnblogs\\.com" }))
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());

			// dowload html by http client
			spider.Downloader = new HttpWebRequestDownloader();
			spider.Name = "CNBLOGS";
			// 4 threads 4线程
			spider.ThreadNum = 4;
			spider.TaskId = "cnblogs";
			// traversal deep 遍历深度
			spider.Scheduler.Depth = 3;

			// stop crawler if it can't get url from the scheduler after 30000 ms 当爬虫连续30秒无法从调度中心取得需要采集的链接时结束.
			spider.EmptySleepTime = 30000;

			// start crawler 启动爬虫
			spider.Run();
		}
	}
}