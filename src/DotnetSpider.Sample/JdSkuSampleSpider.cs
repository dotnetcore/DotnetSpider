using System;
using System.Collections.Generic;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class JdSkuSampleSpider : EntitySpiderBuilder
	{
		public JdSkuSampleSpider() : base("JdSkuSample", Batch.Now)
		{
		}

		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site
			{
				//HttpProxyPool = new HttpProxyPool(new KuaidailiProxySupplier("快代理API"))
			});
			context.SetThreadNum(1);
			context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_hhmmss"));
			// dowload html by http client
			context.SetDownloader(new HttpClientDownloader());
			// save data to mysql.
			context.AddPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main", new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			context.AddEntityType(typeof(Product));
			return context;
		}

		[Table("test", "sku", TableSuffix.Monday, Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[TargetUrlsSelector(XPaths = new[] { "//span[@class=\"p-num\"]" }, Patterns = new[] { @"&page=[0-9]+&" })]
		public class Product : SpiderEntity
		{
			[PropertyDefine(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[PropertyDefine(Expression = "cat3", Type = SelectorType.Enviroment)]
			public int CategoryId { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[PropertyDefine(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[PropertyDefine(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[PropertyDefine(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[PropertyDefine(Expression = "Monday", Type = SelectorType.Enviroment)]
			public DateTime RunId { get; set; }

			[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment)]
			public DateTime CDate { get; set; }
		}
	}

	public class JdSkuSampleSpider2 : EntitySpiderBuilder
	{
		public JdSkuSampleSpider2() : base("JdSkuSample2", Batch.Now)
		{
		}

		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetThreadNum(1);
			context.SetIdentity("JD_sku_store_test_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
			context.AddPipeline(new MySqlEntityPipeline(null)
			{
				UpdateConnectString = new DbUpdateConnectString
				{
					ConnectString = "Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306",
					QueryString = "SELECT value from `dotnetspider`.`settings` where `type`='ConnectString' and `key`='MySql01' LIMIT 1"
				}
			});
			context.AddStartUrl("http://list.jd.com/list.html?cat=9987,653,655&page=2&JL=6_0_0&ms=5#J_main",
				new Dictionary<string, object> { { "name", "手机" }, { "cat3", "655" } });
			context.AddEntityType(typeof(JdSkuSampleSpider.Product));
			return context;
		}
	}
}