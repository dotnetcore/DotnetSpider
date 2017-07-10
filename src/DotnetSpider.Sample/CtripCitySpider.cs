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
	public class CtripCitySpider : EntitySpider
	{
		public CtripCitySpider() : base("Ctrip_City")
		{
		}

		protected override void MyInit(params string[] arguments)
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

			AddStartUrl("http://www.ctrip.com/");

			AddEntityType(typeof(CtripCity));
		}

		[Table("ctrip", "city", Uniques = new[] { "city_id,run_id" })]
		[EntitySelector(Expression = "//div[@class='city_item']//a")]
		public class CtripCity : SpiderEntity
		{
			[PropertyDefine(Expression = ".", Length = 100)]
			public string name { get; set; }

			[PropertyDefine(Expression = "./@title", Length = 100)]
			public string title { get; set; }

			[PropertyDefine(Expression = "./@data-id", Length = 100)]
			public string city_id { get; set; }

			[PropertyDefine(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}
	}
}
