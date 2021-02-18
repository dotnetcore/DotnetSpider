using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Formatters;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class TestSpider2 : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<TestSpider2>(options =>
			{
				options.Speed = 1;
			});
			builder.UseSerilog();
			builder.UseDownloader<HttpClientDownloader>();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		public TestSpider2(
			IOptions<SpiderOptions> options,
			DependenceServices services,
			ILogger<Spider> logger) : base(
			options,
			services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			await AddRequestsAsync(new Request($"https://news.cnblogs.com/n/page/1/"));
			AddDataFlow(new DataParser<CnblogsEntity>());
			AddDataFlow(GetDefaultStorage());
			// AddDataFlow(new HBaseStorage("http://localhost:8080"));
		}

		protected override SpiderId GenerateSpiderId()
		{
			return new(ObjectId.CreateId().ToString(), "测试爬虫 1");
		}

		[Schema("cnblogs", "news")]
		[EntitySelector(Expression = ".//div[@class='news_block']", Type = SelectorType.XPath)]
		[GlobalValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
		[FollowRequestSelector(Expressions = new[] {"//div[@class='pager']"},
			Patterns = new[] {"news\\.cnblogs\\.com/n/page"})]
		public class CnblogsEntity : EntityBase<CnblogsEntity>
		{
			protected override void Configure()
			{
				HasIndex(x => x.Title);
				HasIndex(x => new {x.WebSite, x.Guid}, true);
			}

			public int Id { get; set; }

			[Required]
			[StringLength(200)]
			[ValueSelector(Expression = "类别", Type = SelectorType.Environment)]
			public string Category { get; set; }

			[Required]
			[StringLength(200)]
			[ValueSelector(Expression = "网站", Type = SelectorType.Environment)]
			public string WebSite { get; set; }

			[StringLength(200)]
			[ValueSelector(Expression = "//title")]
			[ReplaceFormatter(NewValue = "", OldValue = " - 博客园")]
			public string Title { get; set; }

			[StringLength(40)]
			[ValueSelector(Expression = "GUID", Type = SelectorType.Environment)]
			public string Guid { get; set; }

			[ValueSelector(Expression = ".//h2[@class='news_entry']/a")]
			public string News { get; set; }

			[ValueSelector(Expression = ".//h2[@class='news_entry']/a/@href")]
			public string Url { get; set; }

			[ValueSelector(Expression = ".//div[@class='entry_summary']")]
			public string PlainText { get; set; }

			[ValueSelector(Expression = "DATETIME", Type = SelectorType.Environment)]
			public DateTime CreationTime { get; set; }
		}
	}
}
