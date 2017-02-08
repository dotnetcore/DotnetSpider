using System;
using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;

namespace DotnetSpider.Sample
{
	public class CrawlerHtml 
	{
		public static void Run()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };
			for (int i = 1; i < 5; ++i)
			{
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			Spider spider = Spider.Create(site,
				"YOUKU_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				new QueueDuplicateRemovedScheduler(),
				new DefaultPageProcessor())
				.AddPipeline(new FilePipeline())
				.SetThreadNum(2);

			spider.EmptySleepTime = 3000;

			// 启动爬虫
			spider.Run();
		}

		public static void CrossPage()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };
			site.AddStartUrl("http://list.youku.com/category/show/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_pr__h__d_1_p_1.html");
			Spider spider = Spider.Create(site,
				"YOUKU_DEEP_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				new QueueDuplicateRemovedScheduler(),
				new MyPageProcessor())
				.AddPipeline(new FilePipeline())
				.SetThreadNum(2);

			// 启动爬虫
			spider.Run();
		}

		public class MyPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				page.AddResultItem("Html", page.Content);

				// 利用XPATH, Regext等筛选出需要采集的URL
				foreach (var url in page.Selectable.SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue(), null));
				}
			}
		}
	}
}