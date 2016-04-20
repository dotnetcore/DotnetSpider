using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Test.Example
{
	public class JdSkuSampleSpider : ISpiderContext
	{
		public SpiderContextBuilder GetBuilder()
		{
			return new SpiderContextBuilder(new SpiderContext
			{
				SpiderName = "JD sku/store test " + DateTime.Now.ToString("yyyy-MM-dd"),
				CachedSize = 1,
				ThreadNum = 1,
				Site = new Site
				{
					EncodingName = "UTF-8"
				},
				StartUrls = new Dictionary<string, Dictionary<string, object>>
				{
					{"http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
					{"http://list.jd.com/list.html?cat=9987,653,655&page=2&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
					{"http://list.jd.com/list.html?cat=9987,653,655&page=3&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
				},
				Scheduler = new RedisScheduler
				{
					Host = "redis",
					Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
					Port = 6379
				},
				Pipeline = new MysqlPipeline
				{
					ConnectString = "Database='mysql';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"
				}
			}, typeof(Product));
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		public class Product : ISpiderEntity
		{
			public Product()
			{
				DateTime dt = DateTime.Now;
				RunId = new DateTime(dt.Year, dt.Month, 1);
			}

			[StoredAs("category", DataType.String, 20)]
			[PropertyExtractBy(Expression = "name", Type = ExtractType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("cat3", DataType.String, 20)]
			[PropertyExtractBy(Expression = "cat3", Type = ExtractType.Enviroment)]
			public int CategoryId { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertyExtractBy(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("sku", DataType.String, 25)]
			[PropertyExtractBy(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[StoredAs("commentscount", DataType.String, 32)]
			[PropertyExtractBy(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[StoredAs("shopname", DataType.String, 100)]
			[PropertyExtractBy(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[StoredAs("name", DataType.String, 50)]
			[PropertyExtractBy(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[StoredAs("venderid", DataType.String, 25)]
			[PropertyExtractBy(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[StoredAs("jdzy_shop_id", DataType.String, 25)]
			[PropertyExtractBy(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[StoredAs("run_id", DataType.Date)]
			[PropertyExtractBy(Expression = "Monday", Type = ExtractType.Enviroment)]
			public DateTime RunId { get; }

			[PropertyExtractBy(Expression = "Now", Type = ExtractType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}
	}
}
