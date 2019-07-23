using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.EventBus;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Sample.samples
{
	public class CnblogsSpider : Spider
	{
		protected override void Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			AddDataFlow(new CnblogsDataParser()).AddDataFlow(new JsonFileStorage());
			AddRequests("https://news.cnblogs.com/");
		}

		class CnblogsDataParser : DataParser
		{
			public CnblogsDataParser()
			{
				Required = DataParserHelper.CheckIfRequiredByRegex("cnblogs\\.com");
				FollowRequestQuerier = BuildFollowRequestQuerier(DataParserHelper.QueryFollowRequestsByXPath("."));
			}

			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				var news = context.GetSelectable().XPath(".//[@class=\"news_block\"]").Nodes();
				var newsObjs = new List<News>();
				foreach (var item in news)
				{
					var url = item.Select(Selectors.XPath(".//h2[@class=\"news_entry\"]/a/@href")).GetValue();
					var summary = item.Select(Selectors.XPath(".//div[@class=\"entry_summary\"]")).GetValue();
					var views = int.Parse(item.Select(Selectors.XPath(".//span[@class=\"view\"")).GetValue()
						.Replace("", " 人浏览"));
					newsObjs.Add(new News
					{
					});
				}

				//context.AddItem("Title",);
				return Task.FromResult(DataFlowResult.Success);
			}
		}

		public CnblogsSpider(IEventBus mq, IStatisticsService statisticsService, SpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
		{
		}

		class News
		{
			public string Url { get; set; }
			public string Summary { get; set; }
			public int CountOfComments { get; set; }
			public int CountOfViews { get; set; }
		}

		class NewsContent
		{
			public string Url { get; set; }
			public string Summary { get; set; }
			public int CountOfComments { get; set; }
			public int CountOfViews { get; set; }
			public string Content { get; set; }
		}
	}
}