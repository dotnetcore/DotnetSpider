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

				SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				SqlServerEntityPipeline updatePipeline = new SqlServerEntityPipeline(ConnectString);
				var metadat2 = EntitySpider.GenerateEntityMetaData(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadat2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadat2.Name, new List<JObject> { data3 });

				var list = conn.Query<ProductInsert>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

				SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				SqlServerEntityPipeline updatePipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

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

				SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				SqlServerEntityPipeline updatePipeline = new SqlServerEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(ProductUpdate).GetTypeInfo());
				updatePipeline.AddEntity(metadata2);
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category", "4C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

				var list = conn.Query<ProductInsert>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

				SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(Product2).GetTypeInfo());
				insertPipeline.AddEntity(metadata);
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category1", "4C" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2 });

				SqlServerEntityPipeline updatePipeline = new SqlServerEntityPipeline(ConnectString, true);
				var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(Product2Update).GetTypeInfo());
				updatePipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(Product2Update).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "Sku", "110" }, { "Category1", "4C" }, { "Category", "AAAA" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				updatePipeline.Process(metadata2.Name, new List<JObject> { data3 });

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

				SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
				var metadata = EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo());
				insertPipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(ProductInsert).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				// Common data
				JObject data1 = new JObject { { "Sku", "110" }, { "Category", "3C" }, { "Url", "http://jd.com/110" }, { "CDate", "2016-08-13" } };
				JObject data2 = new JObject { { "Sku", "111" }, { "Category", "3C" }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				// Value is null
				JObject data3 = new JObject { { "Sku", "112" }, { "Category", null }, { "Url", "http://jd.com/111" }, { "CDate", "2016-08-13" } };
				insertPipeline.Process(metadata.Name, new List<JObject> { data1, data2, data3 });

				var list = conn.Query<ProductInsert>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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
			SqlServerEntityPipeline insertPipeline = new SqlServerEntityPipeline(ConnectString);
			try
			{
				insertPipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity1).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("Columns set as Primary is not a property of your entity.", e.Message);
			}

			try
			{
				insertPipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity2).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("Columns set as update is not a property of your entity.", e.Message);
			}

			try
			{
				insertPipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity3).GetTypeInfo()));
				throw new SpiderException("TEST FAILED.");
			}
			catch (SpiderException e)
			{
				Assert.AreEqual("There is no column need update.", e.Message);
			}
			var metadata = EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity4).GetTypeInfo());
			insertPipeline.AddEntity(EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity4).GetTypeInfo()));
			Assert.AreEqual(1, insertPipeline.GetUpdateColumns(metadata.Name).Length);
			Assert.AreEqual("Value", insertPipeline.GetUpdateColumns(metadata.Name).First());

			SqlServerEntityPipeline insertPipeline2 = new SqlServerEntityPipeline(ConnectString);
			var metadata2 = EntitySpider.GenerateEntityMetaData(typeof(UpdateEntity5).GetTypeInfo());
			insertPipeline2.AddEntity(metadata2);
			Assert.AreEqual(1, insertPipeline2.GetUpdateColumns(metadata2.Name).Length);
			Assert.AreEqual("Value", insertPipeline2.GetUpdateColumns(metadata2.Name).First());
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


		[Table("test", "sku2", TableSuffix.Today, Primary = "Sku", Indexs = new[] { "Sku,Category1" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product2 : SpiderEntity
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


		[Table("test", "sku2", TableSuffix.Today, Primary = "Sku", UpdateColumns = new[] { "Category" })]
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

		[Table("test", "sku2", Primary = "Sku", UpdateColumns = new[] { "category" })]
		public class UpdateEntity1 : SpiderEntity
		{
			[PropertyDefine(Expression = "key", Type = SelectorType.Enviroment, Length = 100)]
			public string Key { get; set; }

			[PropertyDefine(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }

		}

		[Table("test", "sku2", Primary = "Key", UpdateColumns = new[] { "calue" })]
		public class UpdateEntity2 : SpiderEntity
		{
			[PropertyDefine(Expression = "key", Type = SelectorType.Enviroment, Length = 100)]
			public string Key { get; set; }

			[PropertyDefine(Expression = "value", Type = SelectorType.Enviroment, Length = 100)]
			public string Value { get; set; }
		}

		[Table("test", "sku2", Primary = "Key", UpdateColumns = new[] { "Key" })]
		public class UpdateEntity3 : SpiderEntity
		{
			[PropertyDefine(Expression = "key", Type = SelectorType.Enviroment, Length = 100)]
			public string Key { get; set; }

			[PropertyDefine(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}

		[Table("test", "sku2", Primary = "Key", UpdateColumns = new[] { "Value" })]
		public class UpdateEntity4 : SpiderEntity
		{
			[PropertyDefine(Expression = "key", Type = SelectorType.Enviroment, Length = 100)]
			public string Key { get; set; }

			[PropertyDefine(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}

		[Table("test", "sku2", Primary = "Key", UpdateColumns = new[] { "Value", "Key" })]
		public class UpdateEntity5 : SpiderEntity
		{
			[PropertyDefine(Expression = "key", Type = SelectorType.Enviroment, Length = 100)]
			public string Key { get; set; }

			[PropertyDefine(Expression = "value", Type = SelectorType.Enviroment)]
			public string Value { get; set; }
		}
	}
}
