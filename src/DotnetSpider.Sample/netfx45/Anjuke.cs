using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DotnetSpider.Sample
{
	public class AnjukeLouPan : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			var entitySpider = new EntitySpider(new Core.Site())
			{
				Identity = "Anjuke"
			};

			entitySpider.AddStartUrl("http://hz.fang.anjuke.com/loupan/all/");
			entitySpider.AddEntityType(typeof(Loupan));

			entitySpider.AddEntityPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			return entitySpider;
		}

		[EntitySelector(Expression = "//div[@class='item-mod']")]
		[TargetUrlsSelector(XPaths = new[] { ".//div[@class=\"pagination\"]" }, Patterns = new[] { "http://hz\\.fang\\.anjuke\\.com/loupan/all/", @"all/p[0-9]+" })]
		[Schema("anjuke", "loupan")]
		public class Loupan : ISpiderEntity
		{
			[StoredAs("Name", DataType.String, 100)]
			[PropertySelector(Expression = ".//a[@class='items-name']", Type = SelectorType.XPath)]
			public string Name { get; set; }

			[StoredAs("Url", DataType.Text)]
			[PropertySelector(Expression = ".//a[@class='items-name']/@href")]
			public string Url { get; set; }

			[StoredAs("Price", DataType.String, 100)]
			[PropertySelector(Expression = ".//p[@class='price']", Option = PropertySelector.ValueOption.PlainText)]
			public string Price { get; set; }

			[StoredAs("Tel", DataType.String, 100)]
			[PropertySelector(Expression = ".//p[@class='tel']", Option = PropertySelector.ValueOption.PlainText)]
			public string Tel { get; set; }
		}
	}

	public class AnjukeLouPanDetail : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			var site = new Site();

			using (var conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				var list = conn.Query<AnjukeLouPan.Loupan>("select * from anjuke.loupan");

				foreach (var loupan in list)
				{
					var number = loupan.Url.Split(new string[] { "loupan/" }, StringSplitOptions.RemoveEmptyEntries)[1];
					site.AddStartUrl("http://hz.fang.anjuke.com/loupan/canshu-" + number + "?from=loupan_index_more",
						new Dictionary<string, object>()
						{
							{ "Name",loupan.Name },
							{ "Price",loupan.Price },
							{ "Url",loupan.Url },
							{ "Tel",loupan.Tel },
						});
				}
			}
			var context = new EntitySpider(site);

			context.Identity = "AnjukeLouPanDetail" + DateTime.Today.ToString("yyyy-MM-dd");


			context.AddEntityPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddEntityType(typeof(LoupanInfo));
			return context;
		}

		[Schema("anjuke", "loupan_info")]
		public class LoupanInfo : ISpiderEntity
		{
			[StoredAs("Name", DataType.String, 100)]
			[PropertySelector(Expression = "Name", Type = SelectorType.Enviroment)]
			public string Name { get; set; }

			[StoredAs("Url", DataType.Text)]
			[PropertySelector(Expression = "Url", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[StoredAs("Price", DataType.String, 100)]
			[PropertySelector(Expression = "Price", Type = SelectorType.Enviroment)]
			public string Price { get; set; }

			[StoredAs("Tel", DataType.String, 100)]
			[PropertySelector(Expression = "Tel", Type = SelectorType.Enviroment)]
			public string Tel { get; set; }

			[StoredAs("Kfs", DataType.String, 100)]
			[PropertySelector(Expression = "//a[@soj=\"canshu_left_kfs\"]", Type = SelectorType.XPath)]
			public string Kfs { get; set; }

			[StoredAs("Start", DataType.String, 100)]
			[PropertySelector(Expression = "//div[@class='can-left']//ul[@class='list']/li[5]/div[2]", Type = SelectorType.XPath)]
			public string Start { get; set; }

			[StoredAs("Jiaofang", DataType.String, 100)]
			[PropertySelector(Expression = "//div[@class='can-left']//ul[@class='list']/li[6]/div[2]", Type = SelectorType.XPath)]
			public string Jiaofang { get; set; }

			[StoredAs("Changquan", DataType.String, 100)]
			[PropertySelector(Expression = "//ul[@class='list']/li[2]/div[2]/text()", Type = SelectorType.XPath)]
			public string Changquan { get; set; }

			[StoredAs("Hushu", DataType.String, 100)]
			[PropertySelector(Expression = "//div[@class='can-right']//ul[@class='list']/li[5]/div[2]/text()", Type = SelectorType.XPath)]
			public string Hushu { get; set; }


			[StoredAs("Wuye", DataType.String, 100)]
			[PropertySelector(Expression = "//ul[@class='list']/li[9]/div[2]", Type = SelectorType.XPath)]
			public string Wuye { get; set; }

			[StoredAs("Cheweibi", DataType.String, 100)]
			[PropertySelector(Expression = "//ul[@class='list']/li[11]/div[2]/text()", Type = SelectorType.XPath)]
			public string Cheweibi { get; set; }
		}
	}
}
