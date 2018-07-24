using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Sample.docs
{
	public class CustmizeProcessorAndPipelineSpider
	{
		public static void Run()
		{
			// Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				// Add start/feed urls. 添加初始采集链接
				site.AddRequests($"http://list.youku.com/category/show/c_96_s_1_d_1_p_{i}.html");
			}

			Spider spider = Spider.Create(site,
				// use memoery queue scheduler. 使用内存调度
				new QueueDuplicateRemovedScheduler(),
				// use custmize processor for youku 为优酷自定义的 Processor
				new YoukuPageProcessor())
				// use custmize pipeline for youku 为优酷自定义的 Pipeline
				.AddPipeline(new YoukuPipeline());
			// Start crawler 启动爬虫
			spider.Run();
		}

		private class YoukuPipeline : BasePipeline
		{
			private long _count = 0;

			public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
			{
				foreach (var resultItem in resultItems)
				{
					Console.WriteLine();
					Console.WriteLine("=================================================");
					StringBuilder builder = new StringBuilder();
					foreach (YoukuVideo entry in resultItem.Results["VideoResult"])
					{
						_count++;
						builder.Append($" [YoukuVideo {_count}] {entry.Name}");
					}
					Console.WriteLine(builder);
					Console.WriteLine();
					Console.WriteLine("=================================================");
				}

				// Storage data to DB. 可以自由实现插入数据库或保存到文件
			}
		}

		private class YoukuPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				// 利用 Selectable 查询并构造自己想要的数据对象
				var totalVideoElements = page.Selectable().SelectList(Selectors.XPath("//div[@class='yk-pack pack-film']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath(".//img[@class='quic']/@alt")).GetValue();
					results.Add(video);
				}

				// Save data object by key. 以自定义KEY存入page对象中供Pipeline调用
				page.AddResultItem("VideoResult", results);

				// Add target requests to scheduler. 解析需要采集的URL
				foreach (var url in page.Selectable().SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue()));
				}
			}
		}

		private class YoukuVideo
		{
			public string Name { get; set; }
		}
	}
}
