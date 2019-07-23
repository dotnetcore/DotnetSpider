using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Parser.Formatter;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class EntitySpider : Spider
	{
		protected override void Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			AddDataFlow(new DataParser<CnblogsEntry>()).AddDataFlow(new MySqlEntityStorage(
				StorageType.InsertAndUpdate,
				"Database='mysql';Data Source=zousong.com;password=1qazZAQ!;User ID=root;Port=3306;"));
			AddRequests(
				new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
				new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
		}

		protected override async Task OnExiting()
		{
			var verification = Services.GetRequiredService<Verification>();
			verification.EmailTo=new List<string>{"zlzforever@163.com"};
			verification.Subject = "cnblogs crawler";
			verification.AddSqlLarge("total",
				$"SELECT COUNT(*) FROM cnblogs.cnblogs_entity_model",
				10);
			using (var conn = new MySqlConnection(Options.StorageConnectionString))
			{
				await verification.VerifyAsync(conn);
			}
		}

		[Schema("cnblogs", "cnblogs_entity_model")]
		[EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
		[GlobalValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
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

		public EntitySpider(IEventBus mq, IStatisticsService statisticsService, SpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
		{
		}
	}
}