using System;
using System.Collections.Generic;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class JdSkuSampleSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site
			{
				//HttpProxyPool = new HttpProxyPool(new KuaidailiProxySupplier("http://dev.kuaidaili.com/api/getproxy/?orderid=917184806038194&num=999&b_pcchrome=1&b_pcie=1&b_pcff=1&protocol=1&method=2&an_tr=1&an_an=1&an_ha=1&sep=1"))
			})
			{
				UserId = "86Research",
				TaskGroup = "JdSkuSampleSpider"
			};
			context.SetThreadNum(1);
			context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_hhmmss"));
			context.AddEntityPipeline(
				new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main",
				new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			context.AddEntityType(typeof(Product), new TargetUrlExtractor
			{
				Region = new BaseSelector { Type = SelectorType.XPath, Expression = "//span[@class=\"p-num\"]" },
				Patterns = new List<string> { @"&page=[0-9]+&" }
			});
			return context;
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		public class Product : ISpiderEntity
		{
			[StoredAs("category", DataType.String, 20)]
			[PropertySelector(Expression = "name", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("cat3", DataType.String, 20)]
			[PropertySelector(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertySelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("sku", DataType.String, 25)]
			[PropertySelector(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[StoredAs("commentscount", DataType.String, 32)]
			[PropertySelector(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[StoredAs("shopname", DataType.String, 100)]
			[PropertySelector(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[StoredAs("name", DataType.String, 50)]
			[PropertySelector(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[StoredAs("venderid", DataType.String, 25)]
			[PropertySelector(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[StoredAs("jdzy_shop_id", DataType.String, 25)]
			[PropertySelector(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[StoredAs("run_id", DataType.Date)]
			[PropertySelector(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }

			[PropertySelector(Expression = "Now", Type = SelectorType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}
	}

	public class JdSkuSampleSpider2 : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetThreadNum(1);
			context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
			context.AddEntityPipeline(new MySqlEntityPipeline
			{
				UpdateConnectString = new DbUpdateConnectString
				{
					ConnectString = "Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306",
					QueryString = "SELECT value from `dotnetspider`.`settings` where `type`='ConnectString' and `key`='MySql01' LIMIT 1"
				}
			});
			context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main",
				new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			context.AddEntityType(typeof(JdSkuSampleSpider.Product), new TargetUrlExtractor
			{
				Region = new BaseSelector { Type = SelectorType.XPath, Expression = "//span[@class=\"p-num\"]" },
				Patterns = new List<string> { @"&page=[0-9]+&" }
			});
			return context;
		}
	}
}