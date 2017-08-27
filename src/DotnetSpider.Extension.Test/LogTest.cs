using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Monitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DotnetSpider.Extension.Test
{
	internal class CountResult
	{
		public int Count { get; set; }
	}

	[TestClass]
	public class LogTest
	{
		[TestMethod]
		public void DatebaseLogAndStatus()
		{
			string id = Guid.NewGuid().ToString("N");
			string taskGroup = Guid.NewGuid().ToString("N");
			string userId = Guid.NewGuid().ToString("N");

			using (Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
				id,
				new QueueDuplicateRemovedScheduler(),
				new TestPageProcessor()))
			{
				spider.AddPipeline(new TestPipeline());
				spider.ThreadNum = 1;
				for (int i = 0; i < 5; i++)
				{
					spider.AddStartUrl("http://www.baidu.com/" + i);
				}
				spider.Monitor = new DbMonitor(id);
				spider.Run();
			}
			using (var conn = (Core.Environment.SystemConnectionStringSettings.GetDbConnection()))
			{
				Assert.AreEqual(15, conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.log where identity='{id}'").First().Count);
				Assert.AreEqual(1, conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM dotnetspider.status where identity='{id}'").First().Count);
			}
		}

		internal class TestPipeline : BasePipeline
		{
			public override void Process(params ResultItems[] resultItems)
			{
				foreach (var resultItem in resultItems)
				{
					foreach (var entry in resultItem.Results)
					{
						Console.WriteLine($"{entry.Key}:{entry.Value}");
					}
				}
			}
		}

		internal class TestPageProcessor : BasePageProcessor
		{
			protected override void Handle(Page page)
			{
				page.Skip = true;
			}
		}
	}
}
