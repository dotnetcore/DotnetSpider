using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core;

namespace DotnetSpider.Sample
{
	public class PlatenoHotelListSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			Name = "Plateno Hotel List";
			Batch = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
			var context = new EntitySpider(new Site
			{
				Headers = new Dictionary<string, string>
					{
						{"X-Requested-With","XMLHttpRequest" },
						{"Accept-Encoding","gzip, deflate" },
						{"Accept-Language","zh-CN,zh;q=0.8" },
						{"Content-Type","application/x-www-form-urlencoded; charset=UTF-8" }
					},
				Accept = @"application/json, text/javascript, */*; q=0.01",
				UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36"
			})
			{
				UserId = "86Research",
				TaskGroup = "Plateno",

				CachedSize = 1,
				ThreadNum = 1,
				Scheduler = new QueueDuplicateRemovedScheduler(),
				PrepareStartUrls = new PrepareStartUrls[]{ new CommonDbPrepareStartUrls
				{
					Source = DataSource.MySql,
					ConnectString = "Database='test';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306",
					QueryString = "SELECT * FROM plateno.plateno_city;",
					Columns = new []
					{
						new DataColumn { Name = "city_id" },
						new DataColumn { Name = "city_name",Formatters = new Formatter[]
						{
							new UrlEncodeFormater
							{
								Encoding = "UTF-8"
							}
						}}
					},
					FormateStrings = new List<string> {
						"http://www.plateno.com/hotel/query/ota/basic"
					},
					Method = "POST",
					PostBody = "searchKeywordFlag=0&keyword=&city={1}&cityCode={0}&bizlat=0.0&bizlng=0.0&checkInDate=" + DateTimeUtils.GetCurrentTimeStampString() + "&checkOutDate=" + Convert.ToInt64(DateTimeUtils.GetCurrentTimeStamp() + 86400000) + "&days=1&recommandStarType=&brand=&districtCode=&memberType=&minPrice=0&maxPrice=0&hasRoom=0&isFacility=1&sort=1&page=1&pageSize=20&promotion=0&eventType=1",
					Origin = "http://www.plateno.com",
					Referer = "http://www.plateno.com/list.html?city={1}&cityCode={0}&checkInDate=" + DateTimeUtils.GetCurrentTimeStampString() + "&checkOutDate=" + Convert.ToInt64(DateTimeUtils.GetCurrentTimeStamp() + 86400000)
				}},
				Downloader = new HttpClientDownloader
				{
					DownloadCompleteHandlers = new IDownloadCompleteHandler[]
					{
						new IncrementPostTargetUrlsCreator("page=1"),
					}
				},
				SkipWhenResultIsEmpty = true
			};
			context.AddPipeline(new MySqlEntityPipeline("Database='test';Data Source=86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"));
			context.AddEntityType(typeof(PlatenoHotel));
			context.SetEmptySleepTime(10000);
			return context;
		}

		[Table("plateno", "plateno_city", Uniques = new[] { "city_id,hotel_id,run_id" })]
		[EntitySelector(Expression = "$.result.data[*]", Type = SelectorType.JsonPath)]
		public class PlatenoHotel : ISpiderEntity
		{
			//[StoredAs("category", DataType.String, 20)]
			[PropertyDefine(Expression = "city_id", Type = SelectorType.Enviroment, Length = 20)]
			public string city_id { get; set; }

			[PropertyDefine(Expression = "city_name", Type = SelectorType.Enviroment, Length = 20)]
			public string city_name { get; set; }

			[PropertyDefine(Expression = "$.districtCode", Type = SelectorType.JsonPath, Length = 20)]
			public string district_id { get; set; }

			[PropertyDefine(Expression = "$.districtName", Type = SelectorType.JsonPath, Length = 20)]
			public string district_name { get; set; }

			[PropertyDefine(Expression = "$.innId", Type = SelectorType.JsonPath, Length = 20)]
			public string hotel_id { get; set; }

			[PropertyDefine(Expression = "$.innName", Type = SelectorType.JsonPath, Length = 30)]
			public string hotel_name { get; set; }

			[PropertyDefine(Expression = "$.innGrade", Type = SelectorType.JsonPath, Length = 10)]
			public string hotel_level { get; set; }

			[PropertyDefine(Expression = "$.brandId", Type = SelectorType.JsonPath, Length = 10)]
			public string brand_id { get; set; }

			[PropertyDefine(Expression = "$.brandType", Type = SelectorType.JsonPath, Length = 10)]
			public string brand_type { get; set; }

			[PropertyDefine(Expression = "$.address", Type = SelectorType.JsonPath)]
			public string address { get; set; }

			[PropertyDefine(Expression = "$.score", Type = SelectorType.JsonPath, Length = 10)]
			public string score { get; set; }

			[PropertyDefine(Expression = "$.innPhone", Type = SelectorType.JsonPath, Length = 20)]
			public string phone { get; set; }

			[PropertyDefine(Expression = "$.blng", Type = SelectorType.JsonPath, Length = 30)]
			public string b_lng { get; set; }

			[PropertyDefine(Expression = "$.blat", Type = SelectorType.JsonPath, Length = 30)]
			public string b_lat { get; set; }

			[PropertyDefine(Expression = "$.glng", Type = SelectorType.JsonPath, Length = 30)]
			public string g_lng { get; set; }

			[PropertyDefine(Expression = "$.glat", Type = SelectorType.JsonPath, Length = 30)]
			public string g_lat { get; set; }

			[PropertyDefine(Expression = "$.bookFlag", Type = SelectorType.JsonPath, Length = 30)]
			public string book_flag { get; set; }

			[PropertyDefine(Expression = "$.status", Type = SelectorType.JsonPath, Length = 30)]
			public string status { get; set; }

			[PropertyDefine(Expression = "$.lowerPrice", Type = SelectorType.JsonPath, Length = 10)]
			public string lower_price { get; set; }

			[PropertyDefine(Expression = "$.rackRate", Type = SelectorType.JsonPath, Length = 10)]
			public string rack_rate { get; set; }

			[PropertyDefine(Expression = "$.actCode", Type = SelectorType.JsonPath, Length = 10)]
			public string act_code { get; set; }

			[PropertyDefine(Expression = "$.act_name", Type = SelectorType.JsonPath, Length = 10)]
			public string act_name { get; set; }

			[PropertyDefine(Expression = "$.hasRoom", Type = SelectorType.JsonPath, Length = 10)]
			public string has_room { get; set; }

			[PropertyDefine(Expression = "$.bizName", Type = SelectorType.JsonPath, Length = 20)]
			public string biz_name { get; set; }

			[PropertyDefine(Expression = "$.advice", Type = SelectorType.JsonPath, Length = 100)]
			public string advice { get; set; }

			[PropertyDefine(Expression = "$.commentNum", Type = SelectorType.JsonPath, Length = 10)]
			public string comment_num { get; set; }

			[PropertyDefine(Expression = "$.busyRoom", Type = SelectorType.JsonPath, Length = 10)]
			public string busy_room { get; set; }

			[PropertyDefine(Expression = "$.highPoint", Type = SelectorType.JsonPath, Length = 10)]
			public string high_point { get; set; }

			[PropertyDefine(Expression = "$.perMonthBooks", Type = SelectorType.JsonPath, Length = 10)]
			public string per_month_books { get; set; }

			[PropertyDefine(Expression = "$.subway", Type = SelectorType.JsonPath, Length = 50)]
			public string subway { get; set; }

			[PropertyDefine(Expression = "$.recommandStarType", Type = SelectorType.JsonPath, Length = 10)]
			public string recommand_star_type { get; set; }

			[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment, Length = 20)]
			//[StoredAs("run_id", DataType.Date)]
			public DateTime run_id { get; set; }

			//[StoredAs("cdate", DataType.Time)]
			public DateTime cdate => DateTime.Now;
		}
	}
}
