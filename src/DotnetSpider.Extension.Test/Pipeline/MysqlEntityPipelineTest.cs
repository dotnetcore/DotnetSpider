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
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, PipelineMode.Update);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product2).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, PipelineMode.Update);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product2).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "AAAA" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product2>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, PipelineMode.Update, true);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product2).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, PipelineMode.Update, true);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product2).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "AAAA" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product2>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				// Common data
				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				// Value is null
				JObject data3 = new JObject { { "sku", "112" }, { "category", null }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2, data3 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline
				{
					UpdateConnectString = new DbUpdateConnectString
					{
						ConnectString = ConnectString,
						QueryString = "SELECT value from `dotnetspider1`.`settings` where `type`='ConnectString' and `key`='MySql01' LIMIT 1"
					}
				};
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };

				insertPipeline.Process(new List<JObject> { data1, data2 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(2, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("111", list[1].Sku);
				conn.Execute("DROP DATABASE IF EXISTS `dotnetspider1`");
			}

			ClearDb();
		}

		[TestMethod]
		public void Clone()
		{
			MySqlEntityPipeline insertipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306");
			var newPipeline1 = (MySqlEntityPipeline)insertipeline.Clone();
			Assert.AreEqual("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306", newPipeline1.ConnectString);
			Assert.AreEqual(PipelineMode.Insert, newPipeline1.Mode);

			MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306", PipelineMode.Update);
			Assert.AreEqual("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306", updatePipeline.ConnectString);
			Assert.AreEqual(PipelineMode.Update, updatePipeline.Mode);
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[Indexes(Primary = "sku", Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		[UpdateColumns("category")]
		public class Product : ISpiderEntity
		{
			[StoredAs("category", DataType.String, 20)]
			[PropertySelector(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertySelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("sku", DataType.String, 20)]
			[PropertySelector(Expression = "./div[1]/a")]
			public string Sku { get; set; }

			[PropertySelector(Expression = "Now", Type = SelectorType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}

		[Schema("test", "sku2", TableSuffix.Today)]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		[Indexes(Primary = "sku,category1")]
		[UpdateColumns("category")]
		public class Product2 : ISpiderEntity
		{
			[StoredAs("category1", DataType.String, 20)]
			[PropertySelector(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category1 { get; set; }

			[StoredAs("category", DataType.String, 20)]
			[PropertySelector(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertySelector(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[StoredAs("sku", DataType.String, 20)]
			[PropertySelector(Expression = "./div[1]/a")]
			public string Sku { get; set; }

			[PropertySelector(Expression = "Now", Type = SelectorType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}
	}
}
