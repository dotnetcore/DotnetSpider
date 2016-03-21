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
	public class JdSkuSampleSpider : BaseTask
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
					ConnectString = "Database='jd';Data Source= 127.0.0.1;User ID=root;Password=;Port=3306",
					Columns = new List<GeneralDbPrepareStartUrls.Column>() { new GeneralDbPrepareStartUrls.Column()
					{
						Name = "url",
						Formatters = new List<JObject>() { new ReplaceFormatter() { OldValue = ".html", NewValue = "" } .ToJObject()}
					} },
					Limit = 1000,
					FormateString = "{0}&page=1&JL=6_0_0",

				}.ToJObject(),
				Scheduler = new QueueScheduler().ToJObject(),
				//Scheduler = new RedisScheduler()
				//{
				//	Host = "127.0.0.1",
				//	Port = 6379,
				//	Password = ""
				//}.ToJObject(),
				Pipeline = new MysqlPipeline()
				{
					ConnectString = "Database='jd';Data Source= 127.0.0.1;User ID=root;Password=;Port=3306"
				}.ToJObject()
			};
		}

		public override HashSet<Type> EntiTypes => new HashSet<Type>() { typeof(Product) };

		[Schema("test", "sku", TableSuffix.Monday)]
		[TargetUrl(new[] { @"page=[0-9]+" }, "//*[@id=\"J_bottomPage\"]")]
		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Index = new[] { "category" }, Primary = "id", Unique = new[] { "category,sku", "sku" }, AutoIncrement = "id")]
		public class Product : BaseEntity
		{
			public Product()
			{
				DateTime dt = DateTime.Now;
				RunId = new DateTime(dt.Year, dt.Month, 1);
			}

			[StoredAs("category", StoredAs.ValueType.String, 20)]
			[PropertyExtractBy(Expression = "name", Type = ExtractType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("cat3", StoredAs.ValueType.String, 20)]
			[PropertyExtractBy(Expression = "cat3", Type = ExtractType.Enviroment)]
			public int CategoryId { get; set; }

			[StoredAs("url", StoredAs.ValueType.Text)]
			[PropertyExtractBy(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("sku", StoredAs.ValueType.String, 25)]
			[PropertyExtractBy(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[StoredAs("commentscount", StoredAs.ValueType.BigInt)]
			[PropertyExtractBy(Expression = "./div[5]/strong/a")]
			public long CommentsCount { get; set; }

			[StoredAs("shopname", StoredAs.ValueType.String, 100)]
			[PropertyExtractBy(Expression = ".//div[@class='p-shop']/@data-shop_name")]
			public string ShopName { get; set; }

			[StoredAs("name", StoredAs.ValueType.String, 50)]
			[PropertyExtractBy(Expression = ".//div[@class='p-name']/a/em")]
			public string Name { get; set; }

			[StoredAs("venderid", StoredAs.ValueType.String, 25)]
			[PropertyExtractBy(Expression = "./@venderid")]
			public string VenderId { get; set; }

			[StoredAs("jdzy_shop_id", StoredAs.ValueType.String, 25)]
			[PropertyExtractBy(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }

			[StoredAs("run_id", StoredAs.ValueType.Date)]
			[PropertyExtractBy(Expression = "Monday", Type = ExtractType.Enviroment)]
			public DateTime RunId { get; }

			[PropertyExtractBy(Expression = "Now", Type = ExtractType.Enviroment)]
			[StoredAs("cdate", StoredAs.ValueType.Time)]
			public DateTime CDate { get; set; }
		}
	}
}
