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
using Xunit;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class MsSqlEntityPipelineTest
	{
		private const string ConnectString = "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

		private void ClearDb()
		{
			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				var tableName = $"sku_{DateTime.Now.ToString("yyyy_MM_dd")}";
				conn.Execute($"USE test; IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP table {tableName};");
			}
		}

		[Fact]
		public void Update()
		{
			ClearDb();

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);
			}

			ClearDb();
		}

		[Fact]
		public void Insert()
		{
			ClearDb();

			using (SqlConnection conn = new SqlConnection(ConnectString))
			{
				ISpider spider = new DefaultSpider("test", new Site());

				MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				// Common data
				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				// Value is null
				JObject data3 = new JObject { { "sku", "112" }, { "category", null }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2, data3 });

				var list = conn.Query<Product>($"use test;select * from sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Equal(null, list[2].Category);
			}

			ClearDb();
		}

		[Fact]
		public void Clone()
		{
			MsSqlEntityPipeline insertPipeline = new MsSqlEntityPipeline(ConnectString);
			var newPipeline1 = (MsSqlEntityPipeline)insertPipeline.Clone();
			Assert.Equal(ConnectString, newPipeline1.ConnectString);
			Assert.Equal(PipelineMode.Insert, newPipeline1.Mode);

			MsSqlEntityPipeline updatePipeline = new MsSqlEntityPipeline(ConnectString, PipelineMode.Update);
			Assert.Equal(ConnectString, updatePipeline.ConnectString);
			Assert.Equal(PipelineMode.Update, updatePipeline.Mode);
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
	}
}
