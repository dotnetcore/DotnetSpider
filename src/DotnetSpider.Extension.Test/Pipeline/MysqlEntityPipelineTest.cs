using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Extension.Test.Pipeline
{
	[TestClass]
	public class MySqlEntityPipelineTest
	{
		private const string ConnectString = "Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306";

		private void ClearDb()
		{
			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
				conn.Execute($"DROP table IF exists test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}");
			}
		}

		[TestMethod]
		public void Update()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("4C", list[0].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void UpdateWhenUnionPrimary()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void UpdateCheckIfSameBeforeUpdate()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("4C", list[0].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void UpdateWhenUnionPrimaryCheckIfSameBeforeUpdate()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);


				JObject data3 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void Insert()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				// Common data
				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				// Value is null
				JObject data3 = new JObject { { "Sku", "112" }, { "Category", null }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2, data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(3, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("111", list[1].Sku);
				Assert.AreEqual(null, list[2].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void UpdateConnectString()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute("CREATE DATABASE IF NOT EXISTS `dotnetspider1` DEFAULT CHARACTER SET utf8;");
				conn.Execute("CREATE TABLE IF NOT EXISTS `dotnetspider1`.`settings` (`id` int(11) NOT NULL AUTO_INCREMENT,`type` varchar(45) NOT NULL,`key` varchar(45) DEFAULT NULL,`value` text,PRIMARY KEY(`id`),UNIQUE KEY `UNIQUE` (`key`,`type`)) ENGINE=InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;");
				try
				{
					conn.Execute("INSERT `dotnetspider1`.`settings` (`value`,`type`,`key`) VALUES (\"Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306\",'ConnectString','MySql01')");
				}
				catch (Exception)
				{
					// ignored
				}
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(null)
				{
					UpdateConnectString = new DbUpdateConnectString
					{
						ConnectString = ConnectString,
						QueryString = "SELECT value from `dotnetspider1`.`settings` where `type`='ConnectString' and `key`='MySql01' LIMIT 1"
					}
				};
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };

				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("111", list[1].Sku);
				conn.Execute("DROP DATABASE IF EXISTS `dotnetspider1`");
			}

			ClearDb();
		}
 

		[Table("test", "sku", TableSuffix.Today, Primary = "Sku", Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class ProductInsert : SpiderEntity
		{
			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}

		[Table("test", "sku", TableSuffix.Today, Primary = "Sku", UpdateColumns = new[] { "Category" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class ProductUpdate : SpiderEntity
		{
			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}
		[Table("test", "sku2", TableSuffix.Today, Primary = "Sku,Category1")]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product2Insert : SpiderEntity
		{
			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category1 { get; set; }

			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}

		[Table("test", "sku2", TableSuffix.Today, Primary = "Sku,Category1", UpdateColumns = new[] { "Category" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product2Update : SpiderEntity
		{
			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category1 { get; set; }

			[PropertyDefine(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[PropertyDefine(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyDefine(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}
	}
}
