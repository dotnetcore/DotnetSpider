using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using System;

namespace DotnetSpider.Sample
{
	public class Situoli
	{
		public static void Run()
		{
			// Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true, DownloadFiles = true };

			// Set start/seed url
			site.AddStartUrl("http://www.situoli.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"Situoli_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				new QueueDuplicateRemovedScheduler(),
				// default page processor will save whole html, and extract urls to target urls via regex
				new DefaultPageProcessor(new[] { "situoli\\.com" }, new string[] { "user=register", "user=login" }))
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline())
				// dowload html by http client
				.SetDownloader(new HttpClientDownloader())
				// 4 threads 4线程
				.SetThreadNum(1);

			// traversal deep 遍历深度
			spider.Deep = 3;

			// stop crawler if it can't get url from the scheduler after 30000 ms 当爬虫连续30秒无法从调度中心取得需要采集的链接时结束.
			spider.EmptySleepTime = 30000;

			// start crawler 启动爬虫
			spider.Run();
		}
	}
}
