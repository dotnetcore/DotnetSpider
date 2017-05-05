using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using DotnetSpider.Core.Downloader;
using System.Text;

namespace DotnetSpider.Sample
{
	public class BaseUsage
	{
		#region Custmize processor and pipeline ��ȫ�Զ���ҳ����������ݹܵ�

		public static void CustmizeProcessorAndPipeline()
		{
			// Config encoding, header, cookie, proxy etc... ����ɼ��� Site ����, ���� Header��Cookie�������
			var site = new Site { EncodingName = "GB2312", RemoveOutboundLinks = true };
			//for (int i = 1; i < 5; ++i)
			//{
			//	// Add start/feed urls. ��ӳ�ʼ�ɼ�����
			//	site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			//}
			site.AddStartUrl("http://www.unistrong.com/");
			Spider spider = Spider.Create(site,
				// use memoery queue scheduler. ʹ���ڴ����
				new QueueDuplicateRemovedScheduler(),
				// use custmize processor for youku Ϊ�ſ��Զ���� Processor
				new YoukuPageProcessor())
				// use custmize pipeline for youku Ϊ�ſ��Զ���� Pipeline
				.AddPipeline(new YoukuPipeline())
				// dowload html by http client
				.SetDownloader(new HttpClientDownloader())
				// 1 thread
				.SetThreadNum(1);

			spider.EmptySleepTime = 3000;

			// Start crawler ��������
			spider.Run();
		}

		public class YoukuPipeline : BasePipeline
		{
			private static long count = 0;

			public override void Process(params ResultItems[] resultItems)
			{
				foreach (var resultItem in resultItems)
				{
					StringBuilder builder = new StringBuilder();
					foreach (YoukuVideo entry in resultItem.Results["VideoResult"])
					{
						count++;
						builder.Append($" [YoukuVideo {count}] {entry.Name}");
					}
					Console.WriteLine(builder);
				}

				// Other actions like save data to DB. ��������ʵ�ֲ������ݿ�򱣴浽�ļ�
			}
		}

		public class YoukuPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
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

				// Save data object by key. ���Զ���KEY����page�����й�Pipeline����
				page.AddResultItem("VideoResult", results);

				// Add target requests to scheduler. ������Ҫ�ɼ���URL
				foreach (var url in page.Selectable.SelectList(Selectors.XPath("//ul[@class='yk-pages']")).Links().Nodes())
				{
					page.AddTargetRequest(new Request(url.GetValue(), null));
				}
			}
		}

		public class YoukuVideo
		{
			public string Name { get; set; }
		}

		#endregion

		#region Crawler pages without traverse �ɼ�ָ��ҳ�治������

		public static void CrawlerPagesWithoutTraverse()
		{
			var site = new Site { EncodingName = "UTF-8", RemoveOutboundLinks = true };
			for (int i = 1; i < 5; ++i)
			{
				site.AddStartUrl("http://" + $"www.youku.com/v_olist/c_97_g__a__sg__mt__lg__q__s_1_r_0_u_0_pt_0_av_0_ag_0_sg__pr__h__d_1_p_{i}.html");
			}

			Spider spider = Spider.Create(site,
				"YOUKU_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				new QueueDuplicateRemovedScheduler(),
				new SimplePageProcessor())
				.AddPipeline(new FilePipeline())
				.SetThreadNum(2);

			spider.EmptySleepTime = 3000;

			// ��������
			spider.Run();
		}

		#endregion

		#region Crawler pages traversal ������վ

		public static void CrawlerPagesTraversal()
		{
			// Config encoding, header, cookie, proxy etc... ����ɼ��� Site ����, ���� Header��Cookie�������
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
				.AddPipeline(new FilePipeline())
				// dowload html by http client
				.SetDownloader(new HttpClientDownloader())
				// 4 threads 4�߳�
				.SetThreadNum(4);

			// traversal deep �������
			spider.Deep = 3;

			// stop crawler if it can't get url from the scheduler after 30000 ms ����������30���޷��ӵ�������ȡ����Ҫ�ɼ�������ʱ����.
			spider.EmptySleepTime = 30000;

			// start crawler ��������
			spider.Run();
		}

		#endregion
	}
}