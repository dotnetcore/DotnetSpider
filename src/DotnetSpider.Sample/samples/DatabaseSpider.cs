using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.MessageQueue;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Sample.samples
{
	public class DatabaseSpider : Spider
	{
		protected override void Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			DownloaderSettings.Type = DownloaderType.HttpClient;
			AddDataFlow(new DatabaseSpiderDataParser()).AddDataFlow(GetDefaultStorage());
			AddRequests(
					new Request("https://news.cnblogs.com/n/page/1/", new Dictionary<string, string> { { "网站", "博客园" } }),
					new Request("https://news.cnblogs.com/n/page/2/", new Dictionary<string, string> { { "网站", "博客园" } }));
		}

		class DatabaseSpiderDataParser : DataParser
		{
			//public DatabaseSpiderDataParser()
			//{
			//	CanParse = DataParserHelper.CanParseByRegex("cnblogs\\.com");
			//	QueryFollowRequests = DataParserHelper.QueryFollowRequestsByXPath(".");
			//}

			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				context.AddItem("URL", context.Response.Request.Url);
				context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());

				#region add mysql database
				var typeName = typeof(EntitySpider.CnblogsEntry).FullName;
				var entity = new EntitySpider.CnblogsEntry();
				context.Add(typeName, entity.GetTableMetadata());
				ParseResult<EntitySpider.CnblogsEntry> items = new ParseResult<EntitySpider.CnblogsEntry>();
				entity.WebSite = context.Response.Request.Url;
				entity.Url = context.Response.Request.Url;
				entity.Title = context.GetSelectable().XPath(".//title").GetValue();
				items.Add(entity);
				context.AddParseItem(typeName, items);
				#endregion
				return Task.FromResult(DataFlowResult.Success);
			}
		}

		public DatabaseSpider(IMessageQueue mq, IStatisticsService statisticsService, ISpiderOptions options, ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
		{
		}
	}
}