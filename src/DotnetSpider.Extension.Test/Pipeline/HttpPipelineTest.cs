//using Dapper;
//using DotnetSpider.Core;
//using DotnetSpider.Selector;
//using DotnetSpider.Extension.Model;
//using DotnetSpider.Extension.Model.Attribute;
//using DotnetSpider.Extension.Pipeline;
//using MySql.Data.MySqlClient;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using DotnetSpider.Core.Infrastructure;
//using Xunit;

//namespace DotnetSpider.Extension.Test.Pipeline
//{
//	public class HttpPipelineTest
//	{
//		private const string ConnectString = "Database='mysql';Data Source=127.0.0.1;User ID=root;Password=;Port=3306;SslMode=None;";

//		[TableInfo("test", "sku", TableNamePostfix.Today, Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
//		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
//		public class ProductInsert
//		{
//			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
//			public string Category { get; set; }

//			[Field(Expression = "./div[1]/a/@href")]
//			public string Url { get; set; }

//			[Field(Expression = "./div[1]/a", Length = 100)]
//			public string Sku { get; set; }
//		}

//		[TableInfo("test", "sku", TableNamePostfix.Today, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "Category" })]
//		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
//		public class ProductUpdate
//		{
//			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
//			public string Category { get; set; }

//			[Field(Expression = "./div[1]/a/@href")]
//			public string Url { get; set; }

//			[Field(Expression = "./div[1]/a", Length = 100)]
//			public string Sku { get; set; }
//		}

//		private void ClearDb()
//		{
//			using (MySqlConnection conn = new MySqlConnection(ConnectString))
//			{
//				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
//				conn.Execute($"DROP table IF exists test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}");
//			}
//		}

//		[Fact]

//		public void Update()
//		{			
//			var result = HttpSender.Request((new HttpRequest
//			{
//				Url = Env.HubServiceUrl
//			}));
//			if (result.StatusCode != HttpStatusCode.OK)
//			{
//				return;
//			}

//			Env.LoadConfiguration("app.service.config");

//			ClearDb();

//			using (MySqlConnection conn = new MySqlConnection(ConnectString))
//			{
//				ISpider spider = new DefaultSpider("test", new Site());

//				HttpMySqlEntityPipeline insertPipeline = new HttpMySqlEntityPipeline();
//				var metadata = new ModelDefine<ProductInsert>();
//				var tableName = Guid.NewGuid().ToString("N");
//				metadata.TableInfo.Name = tableName;
//				insertPipeline.AddEntity(metadata);
//				insertPipeline.Init();

//				var data1 = new ProductInsert { Sku = "210", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//				var data2 = new ProductInsert { Sku = "211", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

//				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2 }, spider);

//				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(ConnectString);
//				var metadata2 = new ModelDefine<ProductUpdate>();
//				metadata2.TableInfo.Name = tableName;
//				updatePipeline.AddEntity(metadata2);

//				var sql = $" select * from `test`.`{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}` where Sku=210";
//				var data3 = conn.Query<ProductUpdate>(sql).First();
//				data3.Category = "4C";
//				updatePipeline.Process(metadata2.Name, new List<dynamic> { data3 }, spider);

//				var list = conn.Query<ProductInsert>($"select * from test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
//				Assert.Equal(2, list.Count);
//				Assert.Equal("210", list[0].Sku);
//				Assert.Equal("4C", list[0].Category);

//				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
//			}
//			Env.LoadConfiguration("app.config");
//		}

//		[Fact]
//		public void Insert()
//		{
//			var result = HttpSender.Request((new HttpRequest
//			{
//				Url = Env.HubServiceUrl
//			}));
//			if (result.StatusCode != HttpStatusCode.OK)
//			{
//				return;
//			}

//			Env.LoadConfiguration("app.service.config");
//			ClearDb();

//			using (MySqlConnection conn = new MySqlConnection(ConnectString))
//			{
//				ISpider spider = new DefaultSpider("test", new Site());

//				HttpMySqlEntityPipeline insertPipeline = new HttpMySqlEntityPipeline();
//				var metadata = new EntityDefine<ProductInsert>();
//				var tableName = Guid.NewGuid().ToString("N");
//				metadata.TableInfo.Name = tableName;
//				insertPipeline.AddEntity(metadata);
//				insertPipeline.Init();

//				var data1 = new ProductInsert { Sku = "210", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//				var data2 = new ProductInsert { Sku = "211", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//				var data3 = new ProductInsert { Sku = "212", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };

//				insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

//				var list = conn.Query<ProductInsert>($"select * from test.`{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")}`").ToList();
//				Assert.Equal(3, list.Count);
//				Assert.Equal("210", list[0].Sku);
//				Assert.Equal("211", list[1].Sku);
//				Assert.Null(list[2].Category);

//				conn.Execute($"DROP TABLE test.{tableName}_{DateTime.Now.ToString("yyyy_MM_dd")};");
//			}
//			Env.LoadConfiguration("app.config");
//		}
//	}
//}
