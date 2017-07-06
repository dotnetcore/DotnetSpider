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
using System.IO;
using System;
using System.Linq;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Sample
{
	public class TaobaoKeywordWatcher : EntitySpider
	{
		public class MyDataHanlder : DataHandler
		{
			protected override JObject HandleDataOject(JObject data, Page page)
			{
				var sold = data.GetValue("sold")?.Value<int>();
				var price = data.GetValue("price").Value<float>();

				if (sold == null)
				{
					data.Add("sold", -1);
					return data;
				}
				else
				{
					if (price >= 100 && price < 5000)
					{
						if (sold <= 1)
						{
							if (!page.MissTargetUrls)
							{
								page.MissTargetUrls = true;
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
							if (!page.MissTargetUrls)
							{
								page.MissTargetUrls = true;
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
							if (!page.MissTargetUrls)
							{
								page.MissTargetUrls = true;
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
		}

		public TaobaoKeywordWatcher() : base("TAOBAO_KEYWORD_WATHCHER")
		{
		}


		protected override void MyInit(params string[] arguments)
		{
			Site = new Site
			{
				Headers = new Dictionary<string, string>
				{
					{ "Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
					{ "Referer", "https://www.taobao.com/?spm=a230r.1.0.0.ebb2eb2VkWVc7"}
				},
				CookiesStringPart = "thw=cn; miid=715530502217916458; tracknick=style9898123; _cc_=VT5L2FSpdA%3D%3D; tg=0; t=fdf1eb945c2d6b41909558f5c373c37e; cookie2=1cb7771c61122989bb7327f9116858cb; v=0; mt=ci=-1_0; cna=wBEiEVwsTwoCAXTrIc4M/zwX; _tb_token_=e38beee05307e; l=AhoatVWMG7a9HNd5Ar0vu7CJ6so0I54m; isg=AlhY8tnotW2k3pghow1NKSZGIYbqQbzLLM8WWZJJ0RNGLfgXOlGMW27LMVzj; uc3=nk2=EEomLiIV%2BYptPBTr&id2=VyySWWIEs2Gx&vt3=F8dARV%2Bke6706b8vtTM%3D&lg2=VT5L2FSpMGV7TQ%3D%3D; existShop=MTQ5NTYxOTEwMA%3D%3D; lgc=style9898123; skt=57e445e7876bfe9c; publishItemObj=Ng%3D%3D; _m_user_unitinfo_=unit|unzbyun; _m_unitapi_v_=1492572565585; _m_h5_tk=a64b9ef97931dc791ae1708fa1293e93_1496410667055; _m_h5_tk_enc=ade4c443f5c9b6358cfb9821ccf02282; UM_distinctid=15c39e8263a835-05097c28e0b965-37624605-1fa400-15c39e8263bbcd; ali_ab=116.235.37.69.1495620049800.4; linezing_session=3vGYfK3a2T0nRJgCZKSJS15W_1497875606644xXAh_3; uc2=wuf=https%3A%2F%2Fpassport.alibaba.com%2Fac%2Fpassword_reset.htm%3FfromSite%3D6%26appName%3Daliyun%26lang%3Dzh_CN; uc1=cookie14=UoW%2BsOlp%2B6aVYg%3D%3D"
			};
			Scheduler = new RedisScheduler(Configuration.RedisConnectString);
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
			};
			ThreadNum = 20;
			SkipWhenResultIsEmpty = true;
			if (!arguments.Contains("noprepare"))
			{
				PrepareStartUrls = new PrepareStartUrls[]
				{
					new BaseDbPrepareStartUrls
					{
						BulkInsert=true,
						ConnectString = Configuration.ConnectString,
						QueryString = "SELECT * FROM taobao.result_keywords",
						Columns = new []
						{
							new DataColumn ("bidwordstr"),
							new DataColumn ("tab")
						},
						FormateStrings = new List<string> { "https://s.taobao.com/search?q={0}&imgfile=&js=1&stats_click=search_radio_all%3A1&ie=utf8&sort=sale-desc&s=0&tab={1}" }
					}
				};
			}
			AddEntityType(typeof(Item), new MyDataHanlder());
		}

		[Table("taobao", "taobao_items", TableSuffix.FirstDayOfThisMonth, Uniques = new[] { "item_id" })]
		[EntitySelector(Expression = "$.mods.itemlist.data.auctions[*]", Type = SelectorType.JsonPath)]
		public class Item : SpiderEntity
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