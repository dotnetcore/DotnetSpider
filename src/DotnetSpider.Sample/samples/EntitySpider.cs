using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Parser.Attribute;
using DotnetSpider.Data.Parser.Formatter;
using DotnetSpider.Data.Storage;
using DotnetSpider.Data.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.MessageQueue;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class EntitySpider : Spider
	{
		public static Task Run()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddLocalMessageQueue();
					services.AddLocalDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddLocalDownloadCenter();
					services.AddSpiderStatisticsCenter(x => x.UseMemory());
				}).Register<EntitySpider>();
			var provider = builder.Build();
			var spider = provider.Create<EntitySpider>();
			return spider.RunAsync();
		}


		protected override void Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			DownloaderSettings.Type = DownloaderType.HttpClient;
			AddDataFlow(new DataParser<CnblogsEntry>()).AddDataFlow(new SqlServerEntityStorage(StorageType.InsertIgnoreDuplicate,"Data Source=.;Initial Catalog=master;User Id=sa;Password='1qazZAQ!'"));
			AddRequests(
				new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
				new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
		}

		[Schema("cnblogs", "cnblogs_entity_model")]
		[EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
		[ValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
		[FollowSelector(XPaths = new[] {"//div[@class='pager']"})]
		public class CnblogsEntry : EntityBase<CnblogsEntry>
		{
			protected override void Configure()
			{
				HasIndex(x => x.Title);
				HasIndex(x => new {x.WebSite, x.Guid}, true);
			}

			public int Id { get; set; }

			[Required]
			[StringLength(200)]
			[ValueSelector(Expression = "类别", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[Required]
			[StringLength(200)]
			[ValueSelector(Expression = "网站", Type = SelectorType.Enviroment)]
			public string WebSite { get; set; }

			[StringLength(200)]
			[ValueSelector(Expression = "//title")]
			[ReplaceFormatter(NewValue = "", OldValue = " - 博客园")]
			public string Title { get; set; }

			[StringLength(40)]
			[ValueSelector(Expression = "GUID", Type = SelectorType.Enviroment)]
			public string Guid { get; set; }

			[ValueSelector(Expression = ".//h2[@class='news_entry']/a")]
			public string News { get; set; }

			[ValueSelector(Expression = ".//h2[@class='news_entry']/a/@href")]
			public string Url { get; set; }

			[ValueSelector(Expression = ".//div[@class='entry_summary']", ValueOption = ValueOption.InnerText)]
			public string PlainText { get; set; }

			[ValueSelector(Expression = "DATETIME", Type = SelectorType.Enviroment)]
			public DateTime CreationTime { get; set; }
		}

		public EntitySpider(IMessageQueue mq, IStatisticsService statisticsService, ISpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
		{
		}
	}
}