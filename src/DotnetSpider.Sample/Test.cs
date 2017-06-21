using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core;

namespace DotnetSpider.Sample
{
	public class JinjiangHotelListSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			var context = new EntitySpider(new Site
			{
				Headers = new Dictionary<string, string>
				{
					{"X-Requested-With","XMLHttpRequest" },
					{"Accept-Encoding","gzip, deflate" },
					{"Accept-Language","zh-CN,zh;q=0.8" },
					{"Content-Type","application/json" },
				},
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
				Accept = "application/json, text/javascript, */*; q=0.01"
			})
			{
				UserId = "86Research",
				TaskGroup = "Jinjiang",
				Identity = "Jinjiang Hotel List " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"),
				CachedSize = 1,
				ThreadNum = 1,
				Scheduler = new QueueDuplicateRemovedScheduler(),
				PrepareStartUrls = new PrepareStartUrls[]
				{
					new CommonDbPrepareStartUrls
					{
						Method = "POST",
						Source = DataSource.MySql,
						ConnectString = "Database='test';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306",
						QueryString = "SELECT * FROM jinjiang.city WHERE city_name='上海' group by city_name;",
						Columns = new []
						{
							new DataColumn { Name = "city_name"}
						},
						FormateStrings = new List<string> {
							"http://www.jinjianginns.com/services/queryHotelInfo"
						},
						Referer = "http://www.jinjianginns.com/HotelSearch?cityName={0}&checkinDate=" + DateTime.Today.ToString("yyyy-MM-dd") + "&checkoutDate=" + DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + "&queryWords=&promoCode=&fulldisname=",
						PostBody = "{{\"page\":\"1\",\"rows\":\"10\",\"minPrice\":\"\",\"maxPrice\":\"\",\"districts\":\"\",\"language\":\"zh-CN\",\"isPromotion\":\"false\",\"isScoreExchange\":\"false\",\"promotionRateCodes\":null,\"destination\":\"{0}\",\"checkInSDate\":\"" + DateTime.Today.ToString("yyyy-MM-dd") + "\",\"checkoutSDate\":\"" + DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + "\",\"chanel\":\"WWW\",\"keywords\":\"\",\"promotionCode\":\"\",\"esZone\":\"\",\"searchZone\":\"\",\"isReceptForeigner\":\"\",\"facilitys\":\"\",\"brandFilters\":\"\",\"ratings\":\"\",\"brands\":\"JJINN,JJDC,JG,BESTAY,BYL,CA\",\"luxuryBrand\":\"JJINN,JJDC,JG,BESTAY,BYL,CA\"}}"
					}
				},
				SkipWhenResultIsEmpty = true,
				Downloader = new HttpClientDownloader
				{
					DownloadCompleteHandlers = new IDownloadCompleteHandler[]
					{
						new ReplaceContentHandler
						{
							OldValue = "\\\"",
							NewValue = "\""
						},
						new ReplaceContentHandler
						{
							OldValue = "\"{",
							NewValue = "{"
						},
						new ReplaceContentHandler
						{
							OldValue = "}\"",
							NewValue = "}"
						},
						new RemoveContentHandler
						{
							RemoveAll = true,
							Start = "TravelGuide",
							StartOffset = -1,
							End = "\",\"",
							EndOffset = 1
						},
						new RemoveContentHandler
						{
							RemoveAll = true,
							Start = "Description",
							StartOffset = -1,
							End = "\",\"",
							EndOffset = 1
						},
						new RemoveContentHandler()
						{
							Start = "PagerNo",
							StartOffset = -2,
							End = ",\"Success\"",
							EndOffset = 10
						},
						new IncrementTargetUrlsCreator("\"page\":\"1\"")
					}
				}
			};
			context.AddPipeline(new MySqlEntityPipeline
			(
				"Database='taobao';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"
			));
			context.AddEntityType(typeof(JinjiangHotel));
			context.SetEmptySleepTime(10000);
			return context;
		}

		[Table("jinjiang", "hotel_list", Uniques = new[] { "city_name,hotel_id,run_id" })]
		[EntitySelector(Expression = "$.HotelInfos[*]", Type = SelectorType.JsonPath)]
		public class JinjiangHotel : SpiderEntity
		{
			[PropertyDefine(Expression = "city_name", Type = SelectorType.Enviroment, Length = 20)]
			public string city_name { get; set; }

			[PropertyDefine(Expression = "$.hotelId", Type = SelectorType.JsonPath, Length = 30)]
			public string hotel_id { get; set; }

			[PropertyDefine(Expression = "$.JjCode", Type = SelectorType.JsonPath, Length = 30)]
			public string jj_code { get; set; }

			[PropertyDefine(Expression = "$.Brand", Type = SelectorType.JsonPath, Length = 30)]
			public string brand { get; set; }

			[PropertyDefine(Expression = "$.HotelName", Type = SelectorType.JsonPath, Length = 30)]
			public string hotel_name { get; set; }

			[PropertyDefine(Expression = "$.HotelIconClass", Type = SelectorType.JsonPath, Length = 30)]
			public string hotel_icon_class { get; set; }

			[PropertyDefine(Expression = "$.MinPrice", Type = SelectorType.JsonPath, Length = 30)]
			public string min_price { get; set; }

			[PropertyDefine(Expression = "$.MaxPrice", Type = SelectorType.JsonPath, Length = 30)]
			public string max_price { get; set; }

			[PropertyDefine(Expression = "$.Latitude", Type = SelectorType.JsonPath, Length = 30)]
			public string lat { get; set; }

			[PropertyDefine(Expression = "$.Longitude", Type = SelectorType.JsonPath, Length = 30)]
			public string lng { get; set; }

			[PropertyDefine(Expression = "$.Rating", Type = SelectorType.JsonPath, Length = 30)]
			public string rating { get; set; }

			[PropertyDefine(Expression = "$.Address", Type = SelectorType.JsonPath)]
			public string address { get; set; }

			[PropertyDefine(Expression = "$.CurrencyType", Type = SelectorType.JsonPath, Length = 10)]
			public string currency_type { get; set; }

			[PropertyDefine(Expression = "$.ReviewCnt", Type = SelectorType.JsonPath, Length = 10)]
			public string review_cnt { get; set; }

			[PropertyDefine(Expression = "$.Province", Type = SelectorType.JsonPath, Length = 30)]
			public string province { get; set; }

			[PropertyDefine(Expression = "$.DistrictName", Type = SelectorType.JsonPath, Length = 30)]
			public string district_name { get; set; }

			[PropertyDefine(Expression = "$.Phone", Type = SelectorType.JsonPath, Length = 30)]
			public string phone { get; set; }

			[PropertyDefine(Expression = "$.RecommendRank", Type = SelectorType.JsonPath, Length = 10)]
			public string recommend_rank { get; set; }

			[PropertyDefine(Expression = "$.IsRecommend", Type = SelectorType.JsonPath, Length = 10)]
			public string is_recommend { get; set; }

			[PropertyDefine(Expression = "$.IsScoreExchangeType", Type = SelectorType.JsonPath, Length = 10)]
			public string is_score_exchange_type { get; set; }

			[PropertyDefine(Expression = "$.IsReceptForeigner", Type = SelectorType.JsonPath, Length = 10)]
			public string is_recept_foreigner { get; set; }

			[PropertyDefine(Expression = "$.SpecialInvoice", Type = SelectorType.JsonPath, Length = 10)]
			public string special_invoice { get; set; }

			[PropertyDefine(Expression = "$.SpecialInvoiceDesc", Type = SelectorType.JsonPath)]
			public string special_invoice_desc { get; set; }

			[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}
	}
}
