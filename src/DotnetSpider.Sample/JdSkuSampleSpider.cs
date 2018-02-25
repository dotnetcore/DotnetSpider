using System;
using System.Collections.Generic;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	[TaskName("JdSkuSampleSpider")]
	public class JdSkuSampleSpider : EntitySpider
	{
		public JdSkuSampleSpider() : base("JdSkuSample", new Site
		{
		})
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Identity = Identity ?? "JD SKU SAMPLE";
			// storage data to mysql, default is mysql entity pipeline, so you can comment this line. Don't miss sslmode.
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));
			AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			AddEntityType<Product>();
		}
	}

	[EntityTable("test", "jd_sku", EntityTable.Monday, Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
	[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
	[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
	public class Product : SpiderEntity
	{
		[PropertyDefine(Expression = "./@data-sku", Length = 100)]
		public string Sku { get; set; }

		[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
		public string Category { get; set; }

		[PropertyDefine(Expression = "cat3", Type = SelectorType.Enviroment)]
		public int CategoryId { get; set; }

		[PropertyDefine(Expression = "./div[1]/a/@href")]
		public string Url { get; set; }

		[PropertyDefine(Expression = "./div[5]/strong/a")]
		public long CommentsCount { get; set; }

		[PropertyDefine(Expression = ".//div[@class='p-shop']/@data-shop_name", Length = 100)]
		public string ShopName { get; set; }

		[PropertyDefine(Expression = "0", Type = SelectorType.Enviroment)]
		public int ShopId { get; set; }

		[PropertyDefine(Expression = ".//div[@class='p-name']/a/em", Length = 100)]
		public string Name { get; set; }

		[PropertyDefine(Expression = "./@venderid", Length = 100)]
		public string VenderId { get; set; }

		[PropertyDefine(Expression = "./@jdzy_shop_id", Length = 100)]
		public string JdzyShopId { get; set; }

		[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
		public DateTime RunId { get; set; }
	}

	public class JdSkuSampleSpider2 : EntitySpider
	{
		public JdSkuSampleSpider2() : base("JdSkuSample2", new Site())
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			ThreadNum = 1;
			Identity = ("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
			AddPipeline(new MySqlEntityPipeline(null)
			{
				UpdateConnectString = new DbConnectionStringSettingsRefresher
				{
					ConnectString = "Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306",
					QueryString = "SELECT value from `dotnetspider`.`settings` where `type`='ConnectString' and `key`='MySql01' LIMIT 1"
				}
			});
			AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main",
				new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			AddEntityType<Product>();
		}
	}
}