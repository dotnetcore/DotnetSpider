using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using Xunit;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Test.Pipeline
{

	public class MySqlEntityPipelineTest
	{
		private const string ConnectString = "Database='mysql';Data Source=127.0.0.1;User ID=root;Password=;Port=3306;SslMode=None;";

		private void ClearDb()
		{
			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
				conn.Execute($"DROP table IF exists test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}");
			}
		}


		public MySqlEntityPipelineTest()
		{
			Env.EnterpiseService = false;
		}

		[Fact]
		public void Update()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = new EntityDefine<ProductInsert>();
				var tableName = Guid.NewGuid().ToString("N").Substring(8, 8);
				metadata.TableInfo.Name = tableName;
				insertPipeline.AddEntity(metadata);
				insertPipeline.Init();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2 }, spider);

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
				var metadata2 = new EntityDefine<ProductUpdate>();
				metadata2.TableInfo.Name = tableName;
				updatePipeline.AddEntity(metadata2);
				updatePipeline.Init();
				var data3 = conn.Query<ProductUpdate>($"use test;select * from {tableName}_{DateTime.Now.ToString("yyyy_MM_dd")} where Sku=110").First();
				data3.Category = "4C";
				updatePipeline.Process(metadata2.Name, new List<dynamic> { data3 }, spider);

				var list = conn.Query<ProductInsert>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);

				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
			}
		}

		[Fact]
		public void UpdatePipelineUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());
				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = new EntityDefine<Product2Insert>();
				var tableName = Guid.NewGuid().ToString("N");
				metadata.TableInfo.Name = tableName;
				insertPipeline.AddEntity(metadata);
				insertPipeline.Init();

				var data1 = new Product2Insert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
				var data2 = new Product2Insert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2 }, spider);

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline();
				var metadata2 = new EntityDefine<Product2Update>();
				metadata2.TableInfo.Name = tableName;
				updatePipeline.AddEntity(metadata2);
				updatePipeline.Init();

				var data3 = conn.Query<Product2Update>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")} where Sku=110").First();
				data3.Category = "4C";

				updatePipeline.Process(metadata2.Name, new List<dynamic> { data3 }, spider);

				var list = conn.Query<Product2Insert>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);

				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
			}
		}

		[Fact]
		public void Insert()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = new EntityDefine<ProductInsert>();
				var tableName = Guid.NewGuid().ToString("N");
				metadata.TableInfo.Name = tableName;
				insertPipeline.AddEntity(metadata);
				insertPipeline.Init();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
				var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

				var list = conn.Query<ProductInsert>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Null(list[2].Category);

				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
			}
		}

		[Fact]
		public void InsertUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = new EntityDefine<Product2Insert>();
				var tableName = Guid.NewGuid().ToString("N");
				metadata.TableInfo.Name = tableName;
				insertPipeline.AddEntity(metadata);
				insertPipeline.Init();

				var data1 = new Product2Insert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
				var data2 = new Product2Insert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
				var data3 = new Product2Insert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

				var list = conn.Query<Product2Insert>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Null(list[2].Category);

				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
			}
		}

		[Fact]
		public void UpdateConnectString()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute("CREATE DATABASE IF NOT EXISTS `dotnetspider1` DEFAULT CHARACTER SET utf8;");
				conn.Execute("CREATE TABLE IF NOT EXISTS `dotnetspider1`.`settings` (`id` int(11) NOT NULL AUTO_INCREMENT,`type` varchar(45) NOT NULL,`key` varchar(45) DEFAULT NULL,`value` text,PRIMARY KEY(`id`),UNIQUE KEY `UNIQUE` (`key`,`type`)) AUTO_INCREMENT = 1");
				try
				{
					conn.Execute("INSERT `dotnetspider1`.`settings` (`value`,`type`,`key`) VALUES (\"Database='mysql';Data Source=127.0.0.1;User ID=root;Password=;Port=3306\",'ConnectString','MySql01')");
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
				var metadata = new EntityDefine<ProductInsert>();
				insertPipeline.AddEntity(metadata);
				insertPipeline.Init();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2 }, spider);

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				conn.Execute("DROP DATABASE IF EXISTS `dotnetspider1`");
			}
		}


		[EntityTable("test", "sku", EntityTable.Today, Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
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

		[EntityTable("test", "sku", EntityTable.Today, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "Category" })]
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

		[EntityTable("test", "sku2", EntityTable.Today, Uniques = new[] { "Sku,Category1" })]
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

		[EntityTable("test", "sku2", EntityTable.Today, Uniques = new[] { "Sku,Category1" }, UpdateColumns = new[] { "Category" })]
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
