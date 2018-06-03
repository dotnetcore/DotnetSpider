using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System.Linq;
using DotnetSpider.Core.Processor;

namespace DotnetSpider.Sample
{
	[TaskName("TaobaoKeywordWatcher")]
	public class TaobaoKeywordWatcher : EntitySpider
	{
		private class MyAfterDownloadHandler : AfterDownloadCompleteHandler
		{
			public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
			{
				var pager = page.Selectable.Select(Selectors.JsonPath("$.mods.pager.status")).GetValue();
				if (pager != "show")
				{
					page.SkipTargetUrls = true;
				}
			}
		}

		private class MyDataHanlder : IDataHandler
		{
			public void Handle(ref dynamic data, Page page)
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
						return;
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
						return;
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
						return;
					}
				}
			}
		}

		public TaobaoKeywordWatcher() : base("TAOBAO_KEYWORD_WATHCHER", new Site
		{
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
			Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("g_page_config = {", "g_srp_loadCss();", 16, 22));
			Downloader.AddAfterDownloadCompleteHandler(new TargetUrlsHandler(new AutoIncrementTargetUrlsExtractor("&s=0", 44)));
			Downloader.AddAfterDownloadCompleteHandler(new MyAfterDownloadHandler());
			SkipTargetUrlsWhenResultIsEmpty = true;
			if (!arguments.Contains("noprepare"))
			{
				//AddStartUrlBuilder(
				//	new DbStartUrlBuilder(Database.MySql, Env.DataConnectionStringSettings.ConnectionString,
				//	"SELECT * FROM taobao.result_keywords limit 10000", new[] { "bidwordstr", "tab" },
				//	"https://s.taobao.com/search?q={0}&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab={1}"));

				AddStartRequest(new Request("https://s.taobao.com/search?q=妙可蓝多&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab=all", new Dictionary<string, dynamic> { { "bidwordstr", "妙可蓝多" } }));
			}
			AddEntityType<TaobaoItem>(new MyDataHanlder());
		}

		[TableInfo("taobao", "taobao_items", TableNamePostfix.FirstDayOfTheMonth, Uniques = new[] { "item_id" })]
		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		private class TaobaoItem
		{
			[Field(Expression = "tab", Type = SelectorType.Enviroment, Length = 20)]
			public string tab { get; set; }

			[Field(Expression = "supercategory", Type = SelectorType.Enviroment, Length = 20)]
			public string team { get; set; }

			[Field(Expression = "bidwordstr", Type = SelectorType.Enviroment, Length = 20)]
			public string bidwordstr { get; set; }

			[Field(Expression = "$.category", Type = SelectorType.Enviroment, Length = 20)]
			public string category { get; set; }

			[Field(Expression = "$.title", Type = SelectorType.JsonPath, Option = FieldOptions.InnerText, Length = 100)]
			public string name { get; set; }

			[Field(Expression = "$.nick", Type = SelectorType.JsonPath, Length = 50)]
			public string nick { get; set; }

			[Field(Expression = "$.view_price", Type = SelectorType.JsonPath, Length = 50)]
			public string price { get; set; }

			[Field(Expression = "$.category", Type = SelectorType.JsonPath, Length = 20)]
			public string cat { get; set; }

			[Field(Expression = "$.icon", Type = SelectorType.JsonPath)]
			public string icon { get; set; }

			[Field(Expression = "$.view_fee", Type = SelectorType.JsonPath, Length = 50)]
			public string fee { get; set; }

			[Field(Expression = "$.item_loc", Type = SelectorType.JsonPath, Length = 50)]
			public string item_loc { get; set; }

			[Field(Expression = "$.shopcard.isTmall", Type = SelectorType.JsonPath)]
			public bool is_Tmall { get; set; }

			[Field(Expression = "$.view_sales", Type = SelectorType.JsonPath, Length = 50)]
			[ReplaceFormatter(NewValue = "", OldValue = "付款")]
			[ReplaceFormatter(NewValue = "", OldValue = "收货")]
			[ReplaceFormatter(NewValue = "", OldValue = "人")]
			public string sold { get; set; }

			[Field(Expression = "$.nid", Type = SelectorType.JsonPath, Length = 50)]
			public string item_id { get; set; }

			[Field(Expression = "$.detail_url", Type = SelectorType.JsonPath)]
			public string url { get; set; }

			[Field(Expression = "$.user_id", Type = SelectorType.JsonPath, Length = 50)]
			public string user_id { get; set; }
		}
	}
}