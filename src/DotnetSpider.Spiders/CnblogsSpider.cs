using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Parser.Formatter;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Spiders
{
	public class CnblogsSpider : Spider
	{
		protected override void Initialize()
		{
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			AddDataFlow(new DataParser<CnblogsEntry>()).AddDataFlow(GetDefaultStorage());
			for (int i = 1; i < 20; ++i)
			{
				AddRequests(
					new Request($"https://news.cnblogs.com/n/page/{i}/", new Dictionary<string, string> {{"网站", "博客园"}})
				);
			}
		}

		[Schema("cnblogs", "cnblogs_entity_model")]
		[EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
		[GlobalValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
		class CnblogsEntry : EntityBase<CnblogsEntry>
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

		public CnblogsSpider(IEventBus mq, IStatisticsService statisticsService, SpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
		{
		}
	}
}