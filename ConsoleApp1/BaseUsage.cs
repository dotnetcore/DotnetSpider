using System;
using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp1
{
	public class BaseUsage
	{
		public static void Run()
		{
			// 注入监控服务
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();

			// 定义要采集的 Site 对象, 可以设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				// 添加初始采集链接
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			// 使用内存Scheduler、自定义PageProcessor、自定义Pipeline创建爬虫
			Spider spider = Spider.Create(site, new MyPageProcessor(), new QueueDuplicateRemovedScheduler()).AddPipeline(new MyPipeline()).SetThreadNum(1);
			spider.EmptySleepTime = 3000;
			// 注册爬虫到监控服务
			SpiderMonitor.Register(spider);

			// 启动爬虫
			spider.Run();
			Console.Read();
		}

		private class MyPipeline : BasePipeline
		{
			public override void Process(ResultItems resultItems)
			{
				foreach (YoukuVideo entry in resultItems.Results["VideoResult"])
				{
					File.AppendAllLines("test.txt", new[] { entry.Name });
				}

				// 可以自由实现插入数据库或保存到文件
			}
		}

		private class MyPageProcessor : IPageProcessor
		{
			public void Process(Page page)
			{
				// 利用 Selectable 查询并构造自己想要的数据对象
				var totalVideoElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='yk-pack pack-film']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath(".//img[@class='quic']/@alt")).GetValue();
					results.Add(video);
				}
				// 以自定义KEY存入page对象中供Pipeline调用
				page.AddResultItem("VideoResult", results);

				foreach (var url in page.Selectable.SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue(), 0, null));
				}
			}

			public Site Site { get; set; }
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
		}
	}
}