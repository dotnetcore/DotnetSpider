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
			// ע���ط���
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();

			// ����Ҫ�ɼ��� Site ����, �������� Header��Cookie�������
			var site = new Site { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				// ��ӳ�ʼ�ɼ�����
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			// ʹ���ڴ�Scheduler���Զ���PageProcessor���Զ���Pipeline��������
			Spider spider = Spider.Create(site, new MyPageProcessor(), new QueueDuplicateRemovedScheduler()).AddPipeline(new MyPipeline()).SetThreadNum(1);
			spider.EmptySleepTime = 3000;
			// ע�����浽��ط���
			SpiderMonitor.Register(spider);

			// ��������
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

				// ��������ʵ�ֲ������ݿ�򱣴浽�ļ�
			}
		}

		private class MyPageProcessor : IPageProcessor
		{
			public void Process(Page page)
			{
				// ���� Selectable ��ѯ�������Լ���Ҫ�����ݶ���
				var totalVideoElements = page.Selectable.SelectList(Selectors.XPath("//div[@class='yk-pack pack-film']")).Nodes();
				List<YoukuVideo> results = new List<YoukuVideo>();
				foreach (var videoElement in totalVideoElements)
				{
					var video = new YoukuVideo();
					video.Name = videoElement.Select(Selectors.XPath(".//img[@class='quic']/@alt")).GetValue();
					results.Add(video);
				}
				// ���Զ���KEY����page�����й�Pipeline����
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