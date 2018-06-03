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

namespace DotnetSpider.Extension.Test.Pipeline
{

	/// <summary>
	/// grant all privileges on *.* to root@localhost identified by '';
	/// flush privileges;
	/// </summary>
	public class MySqlEntityPipelineTest : TestBase
	{
		private void ClearDb()
		{
			using (MySqlConnection conn = new MySqlConnection(DefaultMySqlConnection))
			{
				conn.Execute($"DROP table IF exists test.sku_{DateTime.Now.ToString("yyyy_MM_dd")}");
				conn.Execute($"DROP table IF exists test.sku2_{DateTime.Now.ToString("yyyy_MM_dd")}");
			}
		}


		public MySqlEntityPipelineTest()
		{
			Env.HubService = false;
		}

		[Fact]
		public void Update()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(DefaultMySqlConnection))
			{

				conn.Execute($"DROP TABLE IF EXISTS test.sku;");

				ISpider spider = new DefaultSpider("test", new Site());

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(DefaultMySqlConnection);
				var metadata = new ModelDefine<ProductInsert>();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110" };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111" };

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems.AddOrUpdateResultItem(metadata.Identity, new Tuple<IModel, IEnumerable<dynamic>>(metadata, new dynamic[] {
					data1,
					data2
				}));
				insertPipeline.Process(new ResultItems[] { resultItems }, spider);

				MySqlEntityPipeline updatePipeline = new MySqlEntityPipeline(DefaultMySqlConnection, PipelineMode.Update);
				var metadata2 = new ModelDefine<ProductUpdate>();

				resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems.AddOrUpdateResultItem(metadata2.Identity, new Tuple<IModel, IEnumerable<dynamic>>(metadata2, new dynamic[] {
					 new ProductUpdate { Sku = "1", Category = "4C" }
				}));
				updatePipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query<ProductInsert>($"select * from test.{metadata2.TableInfo.FullName}").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("1", list[0].Sku);
				Assert.Equal("4C", list[0].Category);

				conn.Execute($"DROP TABLE test.{metadata2.TableInfo.FullName};");
			}
		}

		[Fact(DisplayName = "MySqlEntityPipelineInsertByAutoTimestamp")]
		public void InsertByAutoTimestamp()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(DefaultMySqlConnection))
			{
				conn.Execute($"DROP TABLE if exists test.sku;");
				var spider = new DefaultSpider();

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(DefaultMySqlConnection);
				var metadata = new ModelDefine<ProductInsert>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110" };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111" };
				var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111" };

				resultItems.AddOrUpdateResultItem(metadata.Identity, new Tuple<IModel, IEnumerable<dynamic>>(metadata, new dynamic[] {
					data1,
					data2,
					data3
				}));
				insertPipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query($"select * from test.{metadata.TableInfo.FullName}").Select(r => r as IDictionary<string, dynamic>).ToList();


				Assert.Equal(3, list.Count);
				Assert.Equal(5, list[0].Count);
				Assert.Equal(1, list[0]["sku"]);
				Assert.Equal(2, list[1]["sku"]);
				Assert.Equal(DateTime.Now.Date, list[1]["creation_date"]);
				Assert.True(list[1]["creation_time"] > new DateTime(2000, 1, 1));
				Assert.Null(list[2]["category"]);

				conn.Execute($"DROP TABLE test.{metadata.TableInfo.FullName};");
			}
		}

		[Fact(DisplayName = "MySqlEntityPipelineInsert")]
		public void Insert()
		{
			ClearDb();

			using (MySqlConnection conn = new MySqlConnection(DefaultMySqlConnection))
			{
				conn.Execute($"DROP TABLE IF EXISTS test.sku;");
				var spider = new DefaultSpider();

				MySqlEntityPipeline insertPipeline = new MySqlEntityPipeline(DefaultMySqlConnection);
				insertPipeline.AutoTimestamp = false;
				var metadata = new ModelDefine<ProductInsert>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				var data1 = new ProductInsert { Sku = "110", Category = "3C", Url = "http://jd.com/110" };
				var data2 = new ProductInsert { Sku = "111", Category = "3C", Url = "http://jd.com/111" };
				var data3 = new ProductInsert { Sku = "112", Category = null, Url = "http://jd.com/111" };

				resultItems.AddOrUpdateResultItem(metadata.Identity, new Tuple<IModel, IEnumerable<dynamic>>(metadata, new dynamic[] {
					data1,
					data2,
					data3
				}));
				insertPipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query($"select * from test.{metadata.TableInfo.FullName}").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(3, list.Count);
				Assert.Equal(3, list[0].Count);
				Assert.Equal(1, list[0]["sku"]);
				Assert.Equal("3C", list[1]["category"]);
				Assert.Null(list[2]["category"]);

				conn.Execute($"DROP TABLE test.{metadata.TableInfo.FullName};");
			}
		}

		[TableInfo("test", "sku", TableNamePostfix.Today, PrimaryKey = "sku", Indexs = new[] { "Category" }, Uniques = new[] { "Category,Sku", "Sku" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class ProductInsert
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[Field(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}

		[TableInfo("test", "sku", TableNamePostfix.Today, PrimaryKey = "sku", Uniques = new[] { "Sku" }, UpdateColumns = new[] { "Category" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class ProductUpdate
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[Field(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}

		[TableInfo("test", "sku2", TableNamePostfix.Today, Uniques = new[] { "Sku,Category1" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product2Insert
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category1 { get; set; }

			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[Field(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}

		[TableInfo("test", "sku2", TableNamePostfix.Today, Uniques = new[] { "Sku,Category1" }, UpdateColumns = new[] { "Category" })]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		public class Product2Update
		{
			[Field(Expression = "name", Type = SelectorType.Enviroment, Length = 100)]
			public string Category1 { get; set; }

			[Field(Expression = "name", Type = SelectorType.Enviroment)]
			public string Category { get; set; }

			[Field(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[Field(Expression = "./div[1]/a", Length = 100)]
			public string Sku { get; set; }
		}
	}
}
