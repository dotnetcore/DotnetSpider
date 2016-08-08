using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using DotnetSpider.Core.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace DotnetSpider.Test
{
	[TestClass]
	public class SpiderTest
	{
		[TestMethod]
		public void TestStartAndStop()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();

			Core.Spider spider = Core.Spider.Create(new Site() { EncodingName = "UTF-8" }, new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).AddPipeline(new TestPipeline()).SetThreadNum(1);
			Page p = downloader.Download(new Request("http://www.baidu.com/", 2, new Dictionary<string, dynamic>()), spider);
			Console.WriteLine(p.Content);
			spider.Start();
			Thread.Sleep(10000);
			spider.Stop();
			Thread.Sleep(10000);
			spider.Start();
			Thread.Sleep(10000);
		}

		private class TestPipeline : BasePipeline
		{
			public override void Process(ResultItems resultItems)
			{
				foreach (var entry in resultItems.Results)
				{
					Console.WriteLine($"{entry.Key}:{entry.Value}");
				}
			}
		}

		//[Ignore]
		//[TestMethod]
		//public void TestWaitAndNotify()
		//{
		//	for (int i = 0; i < 10000; i++)
		//	{
		//		Console.WriteLine("round " + i);
		//		TestRound();
		//	}
		//}

		private void TestRound()
		{
			Core.Spider spider = Core.Spider.Create(new Site { }, new TestPageProcessor(), new TestScheduler()).SetThreadNum(10);
			spider.Run();
		}

		private class TestScheduler : IScheduler
		{
			private AtomicInteger _count = new AtomicInteger();
			private Random _random = new Random();

			public ISpider Spider
			{
				get; set;
			}

			public void Init(ISpider spider)
			{
				Spider = spider;
			}

			public void Push(Request request)
			{
			}


			public Request Poll()
			{
				lock (this)
				{
					if (_count.Inc() > 1000)
					{
						return null;
					}
					if (_random.Next(100) > 90)
					{
						return null;
					}
					return new Request("test", 1, null);
				}
			}

			public void Dispose()
			{
			}

			public void Load(HashSet<Request> requests)
			{
				throw new NotImplementedException();
			}

			public HashSet<Request> ToList()
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}
		}

		private class TestPageProcessor : IPageProcessor
		{
			public Site Site
			{
				get; set;
			}

			public void Process(Page page)
			{
				page.IsSkip = true;
			}
		}

		public class TestDownloader : BaseDownloader
		{
			public IDownloadHandler DownloadValidation { get; set; }

			public override Page Download(Request request, ISpider spider)
			{
				var page = new Page(request, ContentType.Html);
				page.Content = "";
				return page;
			}

			public int ThreadNum { get; set; }

			public override IDownloader Clone()
			{
				return (IDownloader)MemberwiseClone();
			}
		}
	}
}
