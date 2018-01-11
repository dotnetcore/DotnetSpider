using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Monitor;
using Xunit;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Extension.Test
{
	internal class CountResult
	{
		public int Count { get; set; }
	}


	public class LogTest
	{
		public LogTest()
		{
			Env.EnterpiseService = false;
		}

		[Fact]
		public void DatebaseLogAndStatus()
		{
			string id = Guid.NewGuid().ToString("N");
			string taskGroup = Guid.NewGuid().ToString("N");
			string userId = Guid.NewGuid().ToString("N");

			using (Spider spider = Spider.Create(new Site { EncodingName = "UTF-8" },
				id,
				new QueueDuplicateRemovedScheduler(),
				new TestPageProcessor()))
			{
				spider.Monitor = new MySqlMonitor(spider.TaskId, spider.Identity, true, "Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;");
				spider.AddPipeline(new TestPipeline());
				for (int i = 0; i < 5; i++)
				{
					spider.AddStartUrl("http://www.baidu.com/" + i);
				}
				spider.Run();
			}
			Thread.Sleep(6000);
			using (var conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;"))
			{
				Assert.StartsWith("Crawl complete, cost", conn.Query<Log>($"SELECT * FROM DotnetSpider.Log where Identity='{id}'").Last().message);
				Assert.Equal(1, conn.Query<CountResult>($"SELECT COUNT(*) as Count FROM DotnetSpider.Status where Identity='{id}'").First().Count);
				Assert.Equal("Finished", conn.Query<statusObj>($"SELECT * FROM DotnetSpider.Status where Identity='{id}'").First().status);
			}
		}

		private class TestDownloader : BaseDownloader
		{
			protected override Page DowloadContent(Request request, ISpider spider)
			{
				Console.WriteLine("ok:" + request.Url);
				return new Page(request)
				{
					Content = ""
				};
			}
		}

		class Log
		{
			public string level { get; set; }
			public string message { get; set; }
		}

		class statusObj
		{
			public string status { get; set; }
		}

		internal class TestPipeline : BasePipeline
		{
			public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
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
