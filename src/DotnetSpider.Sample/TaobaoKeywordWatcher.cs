using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

using DotnetSpider.Extension.Scheduler;
using System.Linq;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Sample
{
	[TaskName("TaobaoKeywordWatcher")]
	public class TaobaoKeywordWatcher : EntitySpider
	{
		private class MyDataHanlder : DataHandler<TaobaoItem>
		{
			protected override TaobaoItem HandleDataOject(TaobaoItem data, Page page)
			{
				var price = float.Parse(data.price);
				var sold = int.Parse(data.sold);

				if (price >= 100 && price < 5000)
				{
					if (sold <= 1)
					{
						if (!page.SkipTargetUrls)
						{
							page.SkipTargetUrls = true;
						}
					}
					else
					{
						return data;
					}
				}
				else if (price < 100)
				{
					if (sold <= 5)
					{
						if (!page.SkipTargetUrls)
						{
							page.SkipTargetUrls = true;
						}
					}
					else
					{
						return data;
					}
				}
				else
				{
					if (sold == 0)
					{
						if (!page.SkipTargetUrls)
						{
							page.SkipTargetUrls = true;
						}
					}
					else
					{
						return data;
					}
				}
				return data;
			}
		}

		public TaobaoKeywordWatcher() : base("TAOBAO_KEYWORD_WATHCHER", new Site
		{
			Timeout = 20000,
			Headers = new Dictionary<string, string>
			{
				{ "Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
				{ "Referer", "https://www.taobao.com/"},
				{ "Cache-Control","max-age=0" },
				{ "Upgrade-Insecure-Requests","1" }
			}
		})
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Downloader.AddAfterDownloadCompleteHandler(new SubContentHandler
			{
				StartOffset = 16,
				EndOffset = 22,
				StartPart = "g_page_config = {",
				EndPart = "g_srp_loadCss();"
			});
			Downloader.AddAfterDownloadCompleteHandler(new IncrementTargetUrlsBuilder("&s=0", 44));
			SkipWhenResultIsEmpty = true;
			if (!arguments.Contains("noprepare"))
			{
				//AddStartUrlBuilder(
				//	new DbStartUrlBuilder(Database.MySql, Env.DataConnectionStringSettings.ConnectionString,
				//	"SELECT * FROM taobao.result_keywords limit 10000", new[] { "bidwordstr", "tab" },
				//	"https://s.taobao.com/search?q={0}&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab={1}"));

				AddStartRequest(new Request("https://s.taobao.com/search?q=妙可蓝多&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab=all", new Dictionary<string, dynamic> { { "bidwordstr", "妙可蓝多" } }));
			}
			AddEntityType<TaobaoItem>();
		}

		[EntityTable("taobao", "taobao_items", EntityTable.FirstDayOfCurrentMonth, Uniques = new[] { "item_id" })]
		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		private class TaobaoItem : SpiderEntity
		{
			[PropertyDefine(Expression = "tab", Type = SelectorType.Enviroment, Length = 20)]
			public string tab { get; set; }

			[PropertyDefine(Expression = "supercategory", Type = SelectorType.Enviroment, Length = 20)]
			public string team { get; set; }

			[PropertyDefine(Expression = "bidwordstr", Type = SelectorType.Enviroment, Length = 20)]
			public string bidwordstr { get; set; }

			[PropertyDefine(Expression = "$.category", Type = SelectorType.Enviroment, Length = 20)]
			public string category { get; set; }

			[PropertyDefine(Expression = "$.title", Type = SelectorType.JsonPath, Option = PropertyDefine.Options.PlainText, Length = 100)]
			public string name { get; set; }

			[PropertyDefine(Expression = "$.nick", Type = SelectorType.JsonPath, Length = 50)]
			public string nick { get; set; }

			[PropertyDefine(Expression = "$.view_price", Type = SelectorType.JsonPath, Length = 50)]
			public string price { get; set; }

			[PropertyDefine(Expression = "$.category", Type = SelectorType.JsonPath, Length = 20)]
			public string cat { get; set; }

			[PropertyDefine(Expression = "$.icon", Type = SelectorType.JsonPath)]
			public string icon { get; set; }

			[PropertyDefine(Expression = "$.view_fee", Type = SelectorType.JsonPath, Length = 50)]
			public string fee { get; set; }

			[PropertyDefine(Expression = "$.item_loc", Type = SelectorType.JsonPath, Length = 50)]
			public string item_loc { get; set; }

			[PropertyDefine(Expression = "$.shopcard.isTmall", Type = SelectorType.JsonPath)]
			public bool is_Tmall { get; set; }

			[PropertyDefine(Expression = "$.view_sales", Type = SelectorType.JsonPath, Length = 50)]
			[ReplaceFormatter(NewValue = "", OldValue = "付款")]
			[ReplaceFormatter(NewValue = "", OldValue = "收货")]
			[ReplaceFormatter(NewValue = "", OldValue = "人")]
			public string sold { get; set; }

			[PropertyDefine(Expression = "$.nid", Type = SelectorType.JsonPath, Length = 50)]
			public string item_id { get; set; }

			[PropertyDefine(Expression = "$.detail_url", Type = SelectorType.JsonPath)]
			public string url { get; set; }

			[PropertyDefine(Expression = "$.user_id", Type = SelectorType.JsonPath, Length = 50)]
			public string user_id { get; set; }
		}
	}
}