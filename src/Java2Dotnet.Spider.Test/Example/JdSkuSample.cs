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
	public class JdSkuSampleSpider : SpiderContextBuilder
	{
		protected override SpiderContext CreateSpiderContext()
		{
			return new SpiderContext
			{
				SpiderName = "JD sku/store test " + DateTimeUtils.FirstDayofThisWeek.ToString("yyyy-MM-dd"),
				CachedSize = 1,
				ThreadNum = 1,
				Site = new Site
				{
					EncodingName = "UTF-8"
				},
				PrepareStartUrls = new GeneralDbPrepareStartUrls()
				{
					Source = GeneralDbPrepareStartUrls.DataSource.MySql,
					TableName = "jd.category",
					ConnectString = "Database='jd';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306",
					Columns = new List<GeneralDbPrepareStartUrls.Column>() { new GeneralDbPrepareStartUrls.Column()
					{
						Name = "url",
						Formatters = new List<Formatter>  { new ReplaceFormatter() { OldValue = ".html", NewValue = "" } }
					} },
					Limit = 1000,
					FormateString = "{0}&page=1&JL=6_0_0",

				},
				Scheduler = new QueueScheduler(),
				//Scheduler = new RedisScheduler()
				//{
				//	Host = "127.0.0.1",
				//	Port = 6379,
				//	Password = ""
				//}.ToJObject(),
				Pipeline = new MysqlPipeline()
				{
					ConnectString = "Database='jd';Data Source= 86research.imwork.net;User ID=root;Password=1qazZAQ!;Port=4306"
				}
			};
		}

		protected override HashSet<Type> EntiTypes => new HashSet<Type>() { typeof(Product) };

		[Schema("test", "sku", TableSuffix.Today)]
		[TargetUrl(new[] { @"page=[0-9]+" }, "//*[@id=\"J_bottomPage\"]")]
		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Index = new[] { "category" }, Primary = "id", Unique = new[] { "category,sku", "sku" }, AutoIncrement = "id")]
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
