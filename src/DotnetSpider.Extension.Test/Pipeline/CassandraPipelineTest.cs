//using DotnetSpider.Core;
//using DotnetSpider.Selector;
//using DotnetSpider.Extension.Infrastructure;
//using DotnetSpider.Extension.Model;
//using DotnetSpider.Extension.Model.Attribute;
//using DotnetSpider.Extension.Pipeline;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;
//#if NETSTANDARD
//using System.Runtime.InteropServices;
//#endif

//namespace DotnetSpider.Extension.Test.Pipeline
//{
//	public class CassandraPipelineTest
//	{
//		string connectString = "Host=127.0.0.1";
//		string keyspace = "test";

//		private void ClearDb()
//		{
//			var cluster = CassandraUtil.CreateCluster(connectString);

//			var session = cluster.Connect();
//			session.DeleteKeyspaceIfExists(keyspace);
//		}

//		[Fact(Skip = "aaa")]
//		public void Insert()
//		{
//#if NETSTANDARD
//			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//			{
//				return;
//			}
//#endif
//			ClearDb();

//			ISpider spider = new DefaultSpider("test", new Site());

//			CassandraEntityPipeline insertPipeline = new CassandraEntityPipeline(connectString);
//			var metadata = new EntityDefine<ProductInsert>();
//			insertPipeline.AddEntity(metadata);
//			insertPipeline.Init();

//			// Common data
//			var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//			var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			// Value is null
//			insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

//			var cluster = CassandraUtil.CreateCluster(connectString);

//			var session = cluster.Connect();
//			session.ChangeKeyspace("test");
//			var rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			var results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category")
//				});
//			}
//			Assert.Equal(3, results.Count);

//			Assert.Contains(results, r => r.Sku == "110");
//			Assert.Contains(results, r => r.Sku == "111");
//			Assert.Contains(results, r => r.Sku == "112");
//			Assert.Contains(results, r => r.Category == null);

//			ClearDb();
//		}

//		[Fact(Skip = "aaa")]
//		public void InsertAndIgnoreDuplicate()
//		{
//#if NETSTANDARD
//			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//			{
//				return;
//			}
//#endif
//			ClearDb();

//			ISpider spider = new DefaultSpider("test", new Site());

//			CassandraEntityPipeline insertPipeline = new CassandraEntityPipeline(connectString);
//			var metadata = new EntityDefine<ProductInsert>();
//			insertPipeline.AddEntity(metadata);
//			insertPipeline.Init();

//			// Common data
//			var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//			var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			// Value is null
//			insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

//			var cluster = CassandraUtil.CreateCluster(connectString);

//			var session = cluster.Connect();
//			session.ChangeKeyspace("test");
//			var rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			var results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category"),
//					Id = row.GetValue<Guid>("id")
//				});
//			}
//			insertPipeline.DefaultPipelineModel = PipelineMode.InsertAndIgnoreDuplicate;
//			var sku = results.First().Sku;
//			var data4 = new ProductInsert { Id = results.First().Id, Sku = "113", Category = "asdfasf", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			insertPipeline.Process(metadata.Name, new List<dynamic> { data4 }, spider);

//			rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category")
//				});
//			}
//			Assert.Equal(3, results.Count);
//			Assert.DoesNotContain(results, r => r.Sku == sku);

//			Assert.Contains(results, r => r.Sku == "113");
//			Assert.Contains(results, r => r.Category == "asdfasf");

//			ClearDb();
//		}

//		[Fact(Skip = "aaa")]
//		public void Update()
//		{
//#if NETSTANDARD
//			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//			{
//				return;
//			}
//#endif
//			ClearDb();

//			ISpider spider = new DefaultSpider("test", new Site());

//			CassandraEntityPipeline insertPipeline = new CassandraEntityPipeline(connectString);
//			var metadata = new EntityDefine<ProductInsert>();
//			insertPipeline.AddEntity(metadata);
//			insertPipeline.Init();

//			// Common data
//			var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//			var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			// Value is null
//			insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

//			var cluster = CassandraUtil.CreateCluster(connectString);

//			var session = cluster.Connect();
//			session.ChangeKeyspace("test");
//			var rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			var results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category"),
//					Id = row.GetValue<Guid>("id")
//				});
//			}
//			insertPipeline.DefaultPipelineModel = PipelineMode.InsertAndIgnoreDuplicate;
//			var sku = results.First().Sku;
//			CassandraEntityPipeline updatePipeline = new CassandraEntityPipeline(connectString);
//			var metadata2 = new EntityDefine<ProductUpdate>();
//			updatePipeline.AddEntity(metadata2);
//			updatePipeline.Init();
//			var data4 = new ProductUpdate { Id = results.First().Id, Sku = "113", Category = "asdfasf", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			updatePipeline.Process(metadata2.Name, new List<dynamic> { data4 }, spider);

//			rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category")
//				});
//			}
//			Assert.Equal(3, results.Count);
//			Assert.DoesNotContain(results, r => r.Sku == sku);

//			Assert.Contains(results, r => r.Sku == "113");
//			Assert.Contains(results, r => r.Category == "asdfasf");

//			ClearDb();
//		}

//		[Fact(Skip = "aaa")]
//		public void UpdatePipelineUseAppConfig()
//		{
//#if NETSTANDARD
//			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//			{
//				return;
//			}
//#endif
//			ClearDb();

//			Env.LoadConfiguration("app.cassandra.config");

//			ISpider spider = new DefaultSpider("test", new Site());

//			CassandraEntityPipeline insertPipeline = new CassandraEntityPipeline();
//			var metadata = new EntityDefine<ProductInsert>();
//			insertPipeline.AddEntity(metadata);
//			insertPipeline.Init();

//			// Common data
//			var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110", CDate = new DateTime(2016, 8, 13) };
//			var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			// Value is null
//			insertPipeline.Process(metadata.Name, new List<dynamic> { data1, data2, data3 }, spider);

//			var cluster = CassandraUtil.CreateCluster(connectString);

//			var session = cluster.Connect();
//			session.ChangeKeyspace("test");
//			var rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			var results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category"),
//					Id = row.GetValue<Guid>("id")
//				});
//			}
//			insertPipeline.DefaultPipelineModel = PipelineMode.InsertAndIgnoreDuplicate;
//			var sku = results.First().Sku;
//			CassandraEntityPipeline updatePipeline = new CassandraEntityPipeline();
//			var metadata2 = new EntityDefine<ProductUpdate>();
//			updatePipeline.AddEntity(metadata2);
//			updatePipeline.Init();
//			var data4 = new ProductUpdate { Id = results.First().Id, Sku = "113", Category = "asdfasf", Url = "http://jd.com/111", CDate = new DateTime(2016, 8, 13) };
//			updatePipeline.Process(metadata2.Name, new List<dynamic> { data4 }, spider);

//			rows = session.Execute($"SELECT * FROM test.sku_cassandra_{DateTime.Now.ToString("yyyy_MM_dd")}").GetRows().ToList();
//			results = new List<ProductInsert>();
//			foreach (var row in rows)
//			{
//				results.Add(new ProductInsert
//				{
//					Sku = row.GetValue<string>("sku"),
//					Category = row.GetValue<string>("category")
//				});
//			}
//			Assert.Equal(3, results.Count);
//			Assert.DoesNotContain(results, r => r.Sku == sku);

//			Assert.Contains(results, r => r.Sku == "113");
//			Assert.Contains(results, r => r.Category == "asdfasf");
//			Env.LoadConfiguration("asdfasdf");
//			ClearDb();
//		}


//		[Fact(Skip = "aaa")]
//		public void InsertUseAppConfig()
//		{
//		}

//		[TableInfo("test", "sku_cassandra", TableNamePostfix.Today, Indexs = new[] { "Category" })]
//		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
//		public class ProductInsert : CassandraSpiderEntity
//		{
//			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
//			public string Category { get; set; }

//			[Field(Expression = "./div[1]/a/@href")]
//			public string Url { get; set; }

//			[Field(Expression = "./div[1]/a", Length = 100)]
//			public string Sku { get; set; }
//		}

//		[TableInfo("test", "sku_cassandra", TableNamePostfix.Today, UpdateColumns = new[] { "Category", "Sku" })]
//		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
//		public class ProductUpdate : CassandraSpiderEntity
//		{
//			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
//			public string Category { get; set; }

//			[Field(Expression = "./div[1]/a/@href")]
//			public string Url { get; set; }

//			[Field(Expression = "./div[1]/a", Length = 100)]
//			public string Sku { get; set; }
//		}
//	}
//}
