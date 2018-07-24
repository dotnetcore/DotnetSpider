using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Downloader.AfterDownloadCompleteHandlers;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class DataHandlerSpider
	{
		public static void Run()
		{
			Spider spider = new Spider();
			spider.Run();
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
						if (!page.SkipTargetRequests)
						{
							page.SkipTargetRequests = true;
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
						if (!page.SkipTargetRequests)
						{
							page.SkipTargetRequests = true;
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
						if (!page.SkipTargetRequests)
						{
							page.SkipTargetRequests = true;
						}
					}
					else
					{
						return;
					}
				}
			}
		}

		private class Spider : EntitySpider
		{
			private class MyBeforeProcessorHandler : IBeforeProcessorHandler
			{
				public void Handle(ref Page page)
				{
					var pager = page.Selectable().Select(Selectors.JsonPath("$.mods.pager.status")).GetValue();
					if (pager != "show")
					{
						page.SkipTargetRequests = true;
					}
				}
			}

			public Spider() : base(new Site
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

			protected override void OnInit(params string[] arguments)
			{
				Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("g_page_config = {", "g_srp_loadCss();", 16, 22));
				AddBeforeProcessor(new TargetRequestHandler(new AutoIncrementTargetRequestExtractor("&s=0", 44)));
				AddBeforeProcessor(new MyBeforeProcessorHandler());
				SkipTargetRequestsWhenResultIsEmpty = true;
				AddStartRequest(new Request("https://s.taobao.com/search?q=妙可蓝多&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab=all", new Dictionary<string, dynamic> { { "bidwordstr", "妙可蓝多" } }));
				AddEntityType<TaobaoItem>(new MyDataHanlder());
				AddPipeline(new ConsoleEntityPipeline());
			}
		}

		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		private class TaobaoItem
		{
			[FieldSelector(Expression = "$.view_price", Type = SelectorType.JsonPath, Length = 50)]
			public string price { get; set; }

			[FieldSelector(Expression = "$.category", Type = SelectorType.JsonPath, Length = 20)]
			public string cat { get; set; }

			[FieldSelector(Expression = "$.view_sales", Type = SelectorType.JsonPath, Length = 50)]
			[ReplaceFormatter(NewValue = "", OldValue = "付款")]
			[ReplaceFormatter(NewValue = "", OldValue = "收货")]
			[ReplaceFormatter(NewValue = "", OldValue = "人")]
			public string sold { get; set; }

			[FieldSelector(Expression = "$.nid", Type = SelectorType.JsonPath, Length = 50)]
			public string item_id { get; set; }

			[FieldSelector(Expression = "$.user_id", Type = SelectorType.JsonPath, Length = 50)]
			public string user_id { get; set; }
		}
	}
}
