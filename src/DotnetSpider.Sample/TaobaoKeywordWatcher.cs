using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Scheduler;
using Newtonsoft.Json.Linq;
using DotnetSpider.Extension.Infrastructure;
using System.IO;

namespace DotnetSpider.Sample
{
	public class TaobaoKeywordWatcher : EntitySpiderBuilder
	{
		public class MyDataHanlder : DataHandler
		{
			protected override JObject HandleDataOject(JObject data, Page page)
			{
				int sold = data.GetValue("sold").Value<int>();
				if (sold == 0)
				{
					if (!page.MissTargetUrls)
					{
						page.MissTargetUrls = true;
					}
				}
				return data;
			}
		}

		public TaobaoKeywordWatcher() : base("TaobaoKeywordCheck ", Extension.Infrastructure.Batch.Now)
		{

		}

		protected override EntitySpider GetEntitySpider()
		{
			Site site = new Site();
			using (var reader = new StreamReader(File.OpenRead("taobaokeyword.txt")))
			{
				string keyword;
				while (!string.IsNullOrEmpty(keyword = reader.ReadLine()))
				{
					site.AddStartUrl("https://" + $"s.taobao.com/search?q={keyword}&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab={1}&fs=1&filter_tianmao=tmall", new Dictionary<string, object>
					{
						{ "keyword" , keyword}
					});
				}
			}
			var context = new EntitySpider(site)
			{
				ThreadNum = 5,
				SkipWhenResultIsEmpty = true,
				Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName = DotnetSpider,keepAlive = 8,allowAdmin = True,connectTimeout = 10000,password = 6GS9F2QTkP36GggE0c3XwVwI,abortConnect = True,connectRetry = 20"),
				Downloader = new HttpClientDownloader
				{
					DownloadCompleteHandlers = new IDownloadCompleteHandler[]
					{
							new SubContentHandler
							{
								StartOffset = 16,
								EndOffset = 22,
								Start = "g_page_config = {",
								End = "g_srp_loadCss();"
							},
						new IncrementTargetUrlsCreator("&s=0",null,44)
					}
				}
			};
			context.AddPipeline(new MySqlEntityPipeline("Database = 'mysql'; Data Source = localhost; User ID = root; Password = 1qazZAQ!; Port = 3306"));
			context.AddEntityType(typeof(Item), new MyDataHanlder());
			return context;
		}

		[Table("taobao", "tmall_paper_diaper")]
		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		public class Item : SpiderEntity
		{
			[PropertyDefine(Expression = "keyword", Type = SelectorType.Enviroment, Length = 20)]
			public string shop { get; set; }

			[PropertyDefine(Expression = "$.category", Type = SelectorType.JsonPath, Length = 20)]
			public string category { get; set; }

			[PropertyDefine(Expression = "$.title", Type = SelectorType.JsonPath, Option = PropertyDefine.Options.PlainText, Length = 100)]
			public string name { get; set; }

			[PropertyDefine(Expression = "$.view_price", Type = SelectorType.JsonPath, Length = 50)]
			public string price { get; set; }

			[PropertyDefine(Expression = "$.view_sales", Type = SelectorType.JsonPath, Length = 100)]
			[ReplaceFormatter(NewValue = "", OldValue = "付款")]
			[ReplaceFormatter(NewValue = "", OldValue = "收货")]
			[ReplaceFormatter(NewValue = "", OldValue = "人")]
			public string sold { get; set; }

			[PropertyDefine(Expression = "$.nid", Type = SelectorType.JsonPath, Length = 50)]
			public string item_id { get; set; }

			[PropertyDefine(Expression = "$.detail_url", Type = SelectorType.JsonPath)]
			public string url { get; set; }

			[PropertyDefine(Expression = "$.user_id", Type = SelectorType.JsonPath, Length = 50)]
			public string userid { get; set; }

			[PropertyDefine(Expression = "$.nick", Type = SelectorType.JsonPath, Length = 100)]
			public string nick { get; set; }
		}
	}
}