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
			//https://s.taobao.com/search?q={0}&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab={1}
			var context = new EntitySpider(new Site() { SleepTime = 3000 })
			{
				ThreadNum = 1,
				SkipWhenResultIsEmpty = true,
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
			context.AddPipeline(new MySqlEntityPipeline("Database='taobao';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"));
			context.AddEntityType(typeof(Item), new MyDataHanlder());
			return context;
		}

		//[Table("taobao", "taobao_keyword_need_watch_result1")]
		[Table("taobao", "taobao_need_watch_result_Rebort")]//美瞳表
		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		public class Item : SpiderEntity
		{
			//[StoredAs("tab", DataType.String, 20)]
			[PropertyDefine(Expression = "tab", Type = SelectorType.Enviroment, Length = 20)]
			public string tab { get; set; }

			//[StoredAs("team", DataType.String, 20)]
			[PropertyDefine(Expression = "team", Type = SelectorType.Enviroment, Length = 20)]
			public string team { get; set; }

			//[StoredAs("keyword", DataType.String, 20)]
			[PropertyDefine(Expression = "word", Type = SelectorType.Enviroment, Length = 20)]
			public string keyword { get; set; }

			[PropertyDefine(Expression = "$.title", Type = SelectorType.JsonPath, Option = PropertyDefine.Options.PlainText, Length = 100)]
			//[StoredAs("name", DataType.String, 100)]
			public string name { get; set; }

			[PropertyDefine(Expression = "$.view_price", Type = SelectorType.JsonPath, Length = 50)]
			//[StoredAs("price", DataType.String, 50)]
			public string price { get; set; }

			[PropertyDefine(Expression = "$.view_sales", Type = SelectorType.JsonPath, Length = 100)]
			//[StoredAs("sold", DataType.String, 100)]
			[ReplaceFormatter(NewValue = "", OldValue = "付款")]
			[ReplaceFormatter(NewValue = "", OldValue = "收货")]
			[ReplaceFormatter(NewValue = "", OldValue = "人")]
			public string sold { get; set; }

			[PropertyDefine(Expression = "$.nid", Type = SelectorType.JsonPath, Length = 50)]
			//[StoredAs("item_id", DataType.String, 50)]
			public string item_id { get; set; }

			//[StoredAs("url", DataType.Text)]
			[PropertyDefine(Expression = "$.detail_url", Type = SelectorType.JsonPath)]
			public string url { get; set; }

			[PropertyDefine(Expression = "$.user_id", Type = SelectorType.JsonPath, Length = 50)]
			//[StoredAs("uid", DataType.String, 50)]
			public string uid { get; set; }

			[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment)]
			//[StoredAs("run_id", DataType.Date)]
			public DateTime run_id { get; set; }

			//[StoredAs("cdate", DataType.Time)]
			//[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment)]
			//public DateTime cdate { get; set; }
		}
	}
}