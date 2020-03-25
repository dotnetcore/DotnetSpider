using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Parser.Formatter;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;

namespace DotnetSpider.Sample.samples
{
	public class EntitySpider2 : Spider
	{
		public EntitySpider2(SpiderParameters parameters) : base(parameters)
		{
		}

		protected override async Task Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			AddDataFlow(new MyParser())
				.AddDataFlow(GetDefaultStorage());
			await AddRequests(
				new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> {{"网站", "博客园"}}),
				new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> {{"网站", "博客园"}}));
		}

		class MyParser : DataParser<CnblogsEntry>
		{
			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				// parse your data and add to parse data
				context.AddParseData(typeof(CnblogsEntry).FullName,
					new ParseResult<CnblogsEntry>
					{
						new CnblogsEntry
						{
							Category = "cat1",
							WebSite = "http://cnblogs.com",
							Title = "title",
							CreationTime = DateTime.Now
						}
					});
				return base.Parse(context);
			}
		}

		[Schema("cnblogs", "news")]
		class CnblogsEntry : EntityBase<CnblogsEntry>
		{
			protected override void Configure()
			{
				HasIndex(x => x.Title);
				HasIndex(x => new {x.WebSite, x.Guid}, true);
			}

			public int Id { get; set; }

			public string Category { get; set; }

			public string WebSite { get; set; }

			public string Title { get; set; }

			public string Guid { get; set; }

			public string News { get; set; }

			public string Url { get; set; }

			public string PlainText { get; set; }

			public DateTime CreationTime { get; set; }
		}
	}
}
