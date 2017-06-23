using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace DotnetSpider.Sample
{
	public class HuazhuBrandSpider : EntitySpider
	{
		public HuazhuBrandSpider() : base("Huazhu_Brand")
		{
		}

		protected override void MyInit()
		{
			Site = new Site
			{
				Headers = new Dictionary<string, string>
				{
					{"Cache-Control","max-age=0" },
					{"Upgrade-Insecure-Requests","1" },
					{"Accept-Encoding","gzip, deflate, sdch" },
					{"Accept-Language","zh-CN,zh;q=0.8" }
				},
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
				Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
			};

			using (var conn = new MySqlConnection(Configuration.ConnectString))
			{
				var cities = conn.Query<HuazhuCity>($"SELECT * FROM huazhu.city WHERE run_id='{DateTimeUtils.RunIdOfToday}'").AsList();
				foreach (var city in cities)
				{
					var url = "http://hotels.huazhu.com/?" + $"CityID={city.city_id}&CheckInDate={DateTime.Now.ToString("yyyy-MM-dd")}&CheckOutDate={DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")}";
					AddStartUrl(url, new Dictionary<string, dynamic>
					{
						{"city_name",city.city_name },
						{"city_id",city.city_id },
						{"city_name_initial",city.city_name_initial },
					});
				}
			}

			AddEntityType(typeof(HuazhuBrand));
		}

		[Table("huazhu", "brand", Uniques = new[] { "brand_id", "city_id", "run_id" })]
		[EntitySelector(Expression = "//div[@class=\"itembox Lcfx branditem\"]//label[@class=\"item\"]")]
		public class HuazhuBrand : SpiderEntity
		{
			[PropertyDefine(Expression = "./input/@data-search-code", Length = 10)]
			public string brand_id { get; set; }

			[PropertyDefine(Expression = "./@title", Length = 10)]
			public string brand_name { get; set; }

			[PropertyDefine(Expression = "city_name", Type = SelectorType.Enviroment, Length = 100)]
			public string city_name { get; set; }

			[PropertyDefine(Expression = "city_id", Type = SelectorType.Enviroment, Length = 100)]
			public string city_id { get; set; }

			[PropertyDefine(Expression = "city_name_initial", Type = SelectorType.Enviroment, Length = 100)]
			public string city_name_initial { get; set; }

			[PropertyDefine(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}

		[Table("huazhu", "city", Uniques = new[] { "city_id,run_id" })]
		[EntitySelector(Expression = "$.CityList[*]", Type = SelectorType.JsonPath)]
		public class HuazhuCity : SpiderEntity
		{
			[PropertyDefine(Expression = "$.CityID", Type = SelectorType.JsonPath, Length = 10)]
			public string city_id { get; set; }

			[PropertyDefine(Expression = "$.PrID", Type = SelectorType.JsonPath, Length = 10)]
			public string pr_id { get; set; }

			[PropertyDefine(Expression = "$.CityName", Type = SelectorType.JsonPath, Length = 10)]
			public string city_name { get; set; }

			[PropertyDefine(Expression = "$.CityNameZhLetterInitial", Type = SelectorType.JsonPath, Length = 10)]
			public string city_name_initial { get; set; }

			[PropertyDefine(Expression = "$.Lat", Type = SelectorType.JsonPath, Length = 20)]
			public string lat { get; set; }

			[PropertyDefine(Expression = "$.Lng", Type = SelectorType.JsonPath, Length = 20)]
			public string lng { get; set; }

			[PropertyDefine(Expression = "$.Group", Type = SelectorType.JsonPath, Length = 10)]
			public string group { get; set; }

			[PropertyDefine(Expression = "$.CitySource", Type = SelectorType.JsonPath, Length = 10)]
			public string city_source { get; set; }

			[PropertyDefine(Expression = "$.CityHotelCount", Type = SelectorType.JsonPath, Length = 10)]
			public string city_hotel_count { get; set; }

			[PropertyDefine(Expression = "$.Domestic", Type = SelectorType.JsonPath, Length = 10)]
			public string domestic { get; set; }

			[PropertyDefine(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}
	}
}
