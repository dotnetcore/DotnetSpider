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
using Xunit;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Test.Pipeline
{
	
	public class MySqlEntityPipelineTest
	{
		private const string ConnectString = "Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306;SslMode=None;";


		public MySqlEntityPipelineTest()
		{
		}

		private void ClearDb()
		{
			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
				conn.Execute($"DROP table IF exists test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}");
			}
		}

		[Fact]
		public void Update()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				var data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				var data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				var data3 = new DataObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdatePipelineUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline();
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateWhenUnionPrimary()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityDefine(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateWhenUnionPrimaryUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = EntitySpider.GenerateEntityDefine(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline();
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateCheckIfSameBeforeUpdate()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateCheckIfSameBeforeUpdateUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(null, true);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateWhenUnionPrimaryCheckIfSameBeforeUpdate()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityDefine(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);


				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateWhenUnionPrimaryCheckIfSameBeforeUpdateUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = EntitySpider.GenerateEntityDefine(typeof(Product2Insert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(null, true);
				var metadata2 = EntitySpider.GenerateEntityDefine(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);


				DataObject data3 = new DataObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<DataObject> { data3 });

				var list = conn.Query<Product2Insert>($"select * from test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("AAAA", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void Insert()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				// Common data
				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				// Value is null
				DataObject data3 = new DataObject { { "Sku", "112" }, { "Category", null }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2, data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Null(list[2].Category);
			}

			ClearDb();
		}

		[Fact]
		public void InsertUseAppConfig()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline();
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				// Common data
				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				// Value is null
				DataObject data3 = new DataObject { { "Sku", "112" }, { "Category", null }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2, data3 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Null(list[2].Category);
			}

			ClearDb();
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
				var metadata = EntitySpider.GenerateEntityDefine(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				DataObject data1 = new DataObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				DataObject data2 = new DataObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };

				insertPipeline.Process(metadata.Name, new List<DataObject> { data1, data2 });

				var list = conn.Query<ProductInsert>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				conn.Execute("DROP DATABASE IF EXISTS `dotnetspider1`");
			}

			ClearDb();
		}


		[Table("test", "sku", TableSuffix.Today, Primary = "Sku", Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku" })]
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
