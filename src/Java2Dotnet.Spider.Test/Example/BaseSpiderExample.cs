using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Core.Pipeline;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Selector;
using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Test.Example
{
	public class SpiderExample
	{
		public static void Run
			()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();

			Core.Spider spider = Core.Spider.Create(new MyPageProcessor(), new QueueDuplicateRemovedScheduler()).AddPipeline(new MyPipeline()).SetThreadNum(1);
			var site = new Site() { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				site.AddStartUrl("http://www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_1.html");
			}
			spider.Site = site;
			spider.Start();
		}

		private class MyPipeline : IPipeline
		{
			public void Process(ResultItems resultItems, ISpider spider)
			{
				foreach (YoukuVideo entry in resultItems.Results["VideoResult"])
				{
					Console.WriteLine($"{entry.Name}:{entry.Click}");
				}

				//May be you want to save to database
				// 
			}

			public void Dispose()
			{
			}
		}

		private class MyPageProcessor : IPageProcessor
		{
			public void Process(Page page)
			{
				var totalVideoElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='yk-col3']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath("/div[4]/div[1]/a")).GetValue();
					video.Click = int.Parse(videoElement.Select(Selectors.Css("p-num")).GetValue().ToString());
					results.Add(video);
				}
				page.AddResultItem("VideoResult", results);
			}

			public Site Site => new Site { SleepTime = 0 };
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
			public string Click { get; set; }
		}
	}
}