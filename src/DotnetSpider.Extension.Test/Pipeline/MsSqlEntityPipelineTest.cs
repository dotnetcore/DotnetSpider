using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Extension.Test.Pipeline
{
	[TestClass]
	/// <summary>
	/// CREATE database  test firstly
	/// </summary>
	public class MsSqlEntityPipelineTest
	{
		private const string ConnectString = "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

		private void ClearDb()
		{
			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				var tableName = $"sku_{DateTime.Now.ToString("yyyy_MM_dd")}";
				var tableName2 = $"sku2_{DateTime.Now.ToString("yyyy_MM_dd")}";
				conn.Execute("if not exists(select * from sys.databases where name = 'test') create database test;");
				conn.Execute($"USE test; IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP table {tableName};");
				conn.Execute($"USE test; IF OBJECT_ID('{tableName2}', 'U') IS NOT NULL DROP table {tableName2};");
			}
		}

		[TestMethod]
		public void Update()
		{
			ClearDb();

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
				updatePipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
				updatePipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "AAAA" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product2>($"use test;select * from sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update, true);
				updatePipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category1", "4C" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update, true);
				updatePipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category1", "4C" }, { "category", "AAAA" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product2>($"use test;select * from sku2_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				// Common data
				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				// Value is null
				JObject data3 = new JObject { { "sku", "112" }, { "category", null }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2, data3 });

				var list = conn.Query<Product>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.AreEqual(3, list.Count);
				Assert.AreEqual("110", list[0].Sku);
				Assert.AreEqual("111", list[1].Sku);
				Assert.AreEqual(null, list[2].Category);
			}

			ClearDb();
		}

		[TestMethod]
		public void DefineUpdateEntity()
		{
			MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
			IEntityPipeline pipeline;
			try
			{
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity1).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("Columns set as primary is not a property of your entity.", e.Message);
			}

			try
			{
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity2).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("Columns set as update is not a property of your entity.", e.Message);
			}

			try
			{
				insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity3).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("There is no column need update.", e.Message);
			}

			insertPipeline.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity4).GetTypeInfo()));
			Assert.AreEqual(1, insertPipeline.GetUpdateColumns().Count);
			Assert.AreEqual("value", insertPipeline.GetUpdateColumns().First().Name);

			MsSqlEntityPipeline insertPipeline2 = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
			insertPipeline2.InitEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity5).GetTypeInfo()));
			Assert.AreEqual(1, insertPipeline2.GetUpdateColumns().Count);
			Assert.AreEqual("value", insertPipeline2.GetUpdateColumns().First().Name);
		}

		[TestMethod]
		public void Clone()
		{
			MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
			var newPipeline1 = (MsSqlEntityPipeline)insertPipeline.Clone();
			Assert.AreEqual(ConnectString, newPipeline1.ConnectString);
			Assert.AreEqual(PipelineMode.Insert, newPipeline1.Mode);

			MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
			Assert.AreEqual(ConnectString, updatePipeline.ConnectString);
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

		[Schema("test", "sku2")]
		[Indexes(Primary = "sku")]
		[UpdateColumns("category")]
		public class UpdateEntity1 : ISpiderEntity
		{
			[StoredAs("key", DataType.String, 20)]
			[PropertySelector(Expression = "key", Type = SelectorType.Enviroment)]
			public string Key { get; set; }

			[StoredAs("value", DataType.String, 20)]
			[PropertySelector(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }

		}

		[Schema("test", "sku2")]
		[Indexes(Primary = "key")]
		[UpdateColumns("category")]
		public class UpdateEntity2 : ISpiderEntity
		{
			[StoredAs("key", DataType.String, 20)]
			[PropertySelector(Expression = "key", Type = SelectorType.Enviroment)]
			public string Key { get; set; }

			[StoredAs("value", DataType.String, 20)]
			[PropertySelector(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}

		[Schema("test", "sku2")]
		[Indexes(Primary = "key")]
		[UpdateColumns("key")]
		public class UpdateEntity3 : ISpiderEntity
		{
			[StoredAs("key", DataType.String, 20)]
			[PropertySelector(Expression = "key", Type = SelectorType.Enviroment)]
			public string Key { get; set; }

			[StoredAs("value", DataType.String, 20)]
			[PropertySelector(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}

		[Schema("test", "sku2")]
		[Indexes(Primary = "key")]
		[UpdateColumns("value")]
		public class UpdateEntity4 : ISpiderEntity
		{
			[StoredAs("key", DataType.String, 20)]
			[PropertySelector(Expression = "key", Type = SelectorType.Enviroment)]
			public string Key { get; set; }

			[StoredAs("value", DataType.String, 20)]
			[PropertySelector(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}

		[Schema("test", "sku2")]
		[Indexes(Primary = "key")]
		[UpdateColumns(new[] { "value", "key" })]
		public class UpdateEntity5 : ISpiderEntity
		{
			[StoredAs("key", DataType.String, 20)]
			[PropertySelector(Expression = "key", Type = SelectorType.Enviroment)]
			public string Key { get; set; }

			[StoredAs("value", DataType.String, 20)]
			[PropertySelector(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}
	}
}
