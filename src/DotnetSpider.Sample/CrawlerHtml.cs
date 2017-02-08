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
			spider.Deep = 2;

			// Æô¶¯ÅÀ³æ
			spider.Run();
		}
	}
}