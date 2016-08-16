using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;
using Xunit;
using Dapper;
using DotnetSpider.Core.Selector;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Test.Pipeline
{
	public class MysqlEntityPipeline
	{
		private void ClearDb()
		{
			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
			}
		}

		[Fact]
		public void Update()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				ISpider spider = new DefaultSpider("test", new Core.Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306");
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306", PipelineMode.Update);
				updatePipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				updatePipeline.InitPipeline(spider);

				JObject data3 = new JObject { { "sku", "110" }, { "category", "4C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				updatePipeline.Process(new List<JObject> { data3 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
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

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				ISpider spider = new DefaultSpider("test", new Core.Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306");
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
			}

			ClearDb();
		}

		[Fact]
		public void UpdateConnectString()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				conn.Execute(Settings.MySqlDatabase);
				conn.Execute(Settings.MySqlSettingTable);
				conn.Execute("update `dotnetspider`.`settings` set `value`=\"Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306\" where `type`='ConnectString' and `key`='MySql01';");
				ISpider spider = new DefaultSpider("test", new Core.Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline
				{
					UpdateConnectString = new DbUpdateConnectString
					{
						ConnectString = "Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306",
						Key = "MySql01"
					}
				};
				insertPipeline.InitiEntity(EntitySpider.ParseEntityMetaData(typeof(Product).GetTypeInfo()));
				insertPipeline.InitPipeline(spider);

				JObject data1 = new JObject { { "sku", "110" }, { "category", "3C" }, { "url", "http://jd.com/110" }, { "cdate", "2016-08-13" } };
				JObject data2 = new JObject { { "sku", "111" }, { "category", "3C" }, { "url", "http://jd.com/111" }, { "cdate", "2016-08-13" } };
				insertPipeline.Process(new List<JObject> { data1, data2 });

				var list = conn.Query<Product>($"select * from test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				conn.Execute(Settings.DropMySqlDatabase);
			}

			ClearDb();
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
