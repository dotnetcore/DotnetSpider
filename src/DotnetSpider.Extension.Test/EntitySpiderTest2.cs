using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Xunit;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Extension.Test
{

	public class EntitySpiderTest2 : TestBase
	{
		private class TestPipeline : DbModelPipeline
		{
			public TestPipeline(string connectString) : base(connectString)
			{
			}

			protected override IDbConnection CreateDbConnection(string connectString)
			{
				throw new NotImplementedException();
			}

			protected override Sqls GenerateSqls(IModel model)
			{
				throw new NotImplementedException();
			}

			protected override void InitDatabaseAndTable(IDbConnection conn, IModel model)
			{
				throw new NotImplementedException();
			}
		}


		[TableInfo("test", "table", Indexs = new[] { "c1" })]
		public class Entity2
		{
			[Field(Expression = "")]
			public string Url { get; set; }
		}

		[TableInfo("test", "table", Uniques = new[] { "c1" })]
		public class Entity3
		{
			[Field(Expression = "")]
			public string Url { get; set; }
		}

		[TableInfo("test", "table", TableNamePostfix.Monday)]
		public class Entity4
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		public class Entity5
		{
			[Field(Expression = "", Length = 100)]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		public class Entity6
		{
			[Field(Expression = "", Length = 255)]
			public string name { get; set; }
		}

		[TableInfo("test", "table")]
		[EntitySelector(Expression = "expression")]
		public class Entity7
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		[EntitySelector(Expression = "expression2", Type = SelectorType.Css)]
		public class Entity8
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		public class Entity9
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "Name3" }, Uniques = new[] { "Name,Name2", "Name2" })]
		public class Entity10
		{
			[Field(Expression = "", Length = 100)]
			public string Name { get; set; }

			[Field(Expression = "", Length = 100)]
			public string Name2 { get; set; }

			[Field(Expression = "", Length = 100)]
			public string Name3 { get; set; }
		}

		[TableInfo("test", "table")]
		public class Entity11
		{
			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			[Field(Expression = "Name")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table12")]
		public class Entity12
		{
			[Field(Expression = "Name")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table13")]
		public class Entity13
		{
			[Field(Expression = "Url")]
			public string Url { get; set; }
		}

		public class Entity14
		{
			[Field(Expression = "Url")]
			public string Url { get; set; }
		}

		[TableInfo("test", "table15")]
		public class Entity15
		{
			[Field(Expression = "Url")]
			public int Int { get; set; }

			[Field(Expression = "Url")]
			public bool Bool { get; set; }

			[Field(Expression = "Url")]
			public long BigInt { get; set; }

			[Field(Expression = "Url")]
			public string String { get; set; }

			[Field(Expression = "Url")]
			public DateTime Time { get; set; }

			[Field(Expression = "Url")]
			public float Float { get; set; }

			[Field(Expression = "Url")]
			public double Double { get; set; }

			[Field(Expression = "Url", Length = 100)]
			public string String1 { get; set; }

			[Field(Expression = "Url", Length = 0)]
			public string String2 { get; set; }

			[Field(Expression = "Url")]
			public decimal Decimal { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "c1" })]
		public class Entity16
		{
			[Field(Expression = "")]
			public int c1 { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "c1" })]
		public class Entity17
		{
			[Field(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[TableInfo("test", "table", Uniques = new[] { "c1" })]
		public class Entity18
		{
			[Field(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[TableInfo("test", "table")]
		public class Entity19
		{
			[Field(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[Fact]
		public void EntitySelector()
		{
			var entity1 = new ModelDefine<Entity7>();
			Assert.Equal("expression", entity1.Selector.Expression);
			Assert.Equal(SelectorType.XPath, entity1.Selector.Type);


			var entity2 = new ModelDefine<Entity8>();
			Assert.Equal("expression2", entity2.Selector.Expression);
			Assert.Equal(SelectorType.Css, entity2.Selector.Type);

			var entity3 = new ModelDefine<Entity9>();
			Assert.Null(entity3.Selector);
			Assert.Equal("test.table", entity3.Identity);
		}

		[Fact]
		public void Indexes()
		{
			var entity1 = new ModelDefine<Entity10>();
			Assert.Equal("Name3", entity1.TableInfo.Indexs[0]);
			Assert.Equal(2, entity1.TableInfo.Uniques.Length);
			Assert.Equal("Name,Name2", entity1.TableInfo.Uniques[0]);
			Assert.Equal("Name2", entity1.TableInfo.Uniques[1]);
		}

		[Fact]
		public void ColumnOfIndexesOverLength()
		{
			try
			{
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;
				var entity = new ModelDefine<Entity17>();

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of index should not large than 256", e.Message);
			}
		}

		[Fact]
		public void ColumnOfUniqueOverLength()
		{
			try
			{
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;
				var entity = new ModelDefine<Entity18>();

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of unique should not large than 256", e.Message);
			}
		}


		[Fact]
		public void CustomePrimary()
		{

		}

		[Fact]
		public void ColumnOfIndexesIsInt()
		{
			EntitySpider context = new DefaultEntitySpider();
			context.Identity = (Guid.NewGuid().ToString("N"));
			context.ThreadNum = 1;
			context.AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306"));
			context.AddEntityType<Entity16>();
		}

		[Fact]
		public void Formater()
		{
			var entity1 = new ModelDefine<Entity11>();
			var fields = entity1.Fields.ToArray();
			var formatters = (fields[0]).Formatters;
			Assert.Equal(2, formatters.Length);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.Equal("a", replaceFormatter.NewValue);
			Assert.Equal("b", replaceFormatter.OldValue);
		}

		[Fact]
		public void Schema()
		{
			var entityMetadata = new ModelDefine<Entity4>();

			Assert.Equal("test", entityMetadata.TableInfo.Database);
			Assert.Equal(TableNamePostfix.Monday, entityMetadata.TableInfo.Postfix);

			var entityMetadata1 = new ModelDefine<Entity14>();
			Assert.Null(entityMetadata1.TableInfo);
		}

		[Fact]
		public void SetNotExistColumnToIndex()
		{
			try
			{
				var entityMetadata = new ModelDefine<Entity2>();
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Columns set as index are not a property of your entity", exception.Message);
			}
		}

		[Fact]
		public void SetNotExistColumnToUnique()
		{
			try
			{
				var entityMetadata = new ModelDefine<Entity3>();

				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Columns set as unique are not a property of your entity", exception.Message);
			}
		}
		[Fact]
		public void MySqlFileEntityPipeline_InsertSql()
		{
			var id = Guid.NewGuid().ToString("N");
			var folder = Path.Combine(Env.BaseDirectory, "mysql", id);
			var path = Path.Combine(folder, "baidu.baidu_search_mysql_file.sql");
			try
			{
				MySqlFileEntityPipelineSpider spider = new MySqlFileEntityPipelineSpider();
				spider.Identity = id;
				spider.Run();

				var lines = File.ReadAllLines(path);
				Assert.Equal(20, lines.Length);
			}
			finally
			{
				if (Directory.Exists(folder))
				{
					Directory.Delete(folder, true);
				}
			}
		}

		class MySqlFileEntityPipelineSpider : EntitySpider
		{
			public MySqlFileEntityPipelineSpider() : base("MySqlFileEntityPipelineSpider")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				EmptySleepTime = 1000;
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType<BaiduSearchEntry>();
				AddPipeline(new MySqlFileEntityPipeline(MySqlFileEntityPipeline.FileType.InsertSql));
			}

			[TableInfo("baidu", "baidu_search_mysql_file")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry
			{
				[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }
			}
		}

		[Fact]
		public void MultiEntitiesInitPipelines()
		{
			EntitySpider spider = new DefaultEntitySpider();
			spider.Identity = (Guid.NewGuid().ToString("N"));
			spider.ThreadNum = 1;
			spider.AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));
			spider.AddPipeline(new MySqlFileEntityPipeline());
			spider.AddPipeline(new ConsoleEntityPipeline());
			spider.AddPipeline(new JsonFileEntityPipeline());

			spider.AddStartUrl("http://baidu.com");
			spider.Monitor = new LogMonitor();
			spider.AddEntityType<Entity13>();
			spider.AddEntityType<Entity12>();

			spider.Run("running-test");

			var entityPipelines = spider.Pipelines.ToList();

			Assert.Equal(4, entityPipelines.Count);

			var pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;", pipeline1.ConnectString);

			Assert.Equal("MySqlFileEntityPipeline", entityPipelines[1].GetType().Name);
			Assert.Equal("ConsoleEntityPipeline", entityPipelines[2].GetType().Name);
			Assert.Equal("JsonFileEntityPipeline", entityPipelines[3].GetType().Name);

			var pipelines = spider.Pipelines.ToList();
			Assert.Equal(4, pipelines.Count);
			ModelPipeline pipeline = (ModelPipeline)pipelines[0];
			//entityPipelines = pipeline.GetEntityPipelines();
			//Assert.Equal(4, entityPipelines.Count);
			//pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			//Assert.Equal("test", pipeline1.GetSchema().Database);
			//Assert.Equal("table13", pipeline1.GetSchema().Name);

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"))
			{
				conn.Execute($"DROP table test.table12");
				conn.Execute($"DROP table test.table13");
			}
		}

		[Fact]
		public void MySqlDataTypeTests()
		{
			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"))
			{
				conn.Execute("drop table if exists test.table15;");

				var spider = new DefaultSpider();

				EntityProcessor<Entity15> processor = new EntityProcessor<Entity15>();

				var pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;");
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems.AddOrUpdateResultItem(processor.Model.Identity, new Tuple<IModel, IEnumerable<dynamic>>(processor.Model, new dynamic[] {
					new Dictionary<string, dynamic>
					{
						{ "int", "1"},
						{ "bool", "1"},
						{ "bigint", "11"},
						{ "string", "aaa"},
						{ "time", "2018-06-12"},
						{ "float", "1"},
						{ "double", "1"},
						{ "string1", "abc"},
						{ "string2", "abcdd"},
						{ "decimal", "1"}
					}
				}));
				pipeline.Process(new ResultItems[] { resultItems }, spider);

				var columns = conn.Query<ColumnInfo>("SELECT COLUMN_NAME as `Name`, COLUMN_TYPE as `Type` FROM information_schema.columns WHERE table_name='table15' AND table_schema = 'test';").ToList(); ;
				Assert.Equal(13, columns.Count);

				Assert.Equal("int".ToLower(), columns[0].Name);
				Assert.Equal("bool".ToLower(), columns[1].Name);
				Assert.Equal("bigint".ToLower(), columns[2].Name);
				Assert.Equal("string".ToLower(), columns[3].Name);
				Assert.Equal("time".ToLower(), columns[4].Name);
				Assert.Equal("float".ToLower(), columns[5].Name);
				Assert.Equal("double".ToLower(), columns[6].Name);
				Assert.Equal("string1".ToLower(), columns[7].Name);
				Assert.Equal("string2".ToLower(), columns[8].Name);
				Assert.Equal("decimal".ToLower(), columns[9].Name);
				Assert.Equal("creation_time".ToLower(), columns[10].Name);
				Assert.Equal("creation_date".ToLower(), columns[11].Name);
				Assert.Equal("id".ToLower(), columns[12].Name);


				Assert.Equal("int(11)", columns[0].Type);
				Assert.Equal("tinyint(1)", columns[1].Type);
				Assert.Equal("bigint(20)", columns[2].Type);
				Assert.Equal("varchar(255)", columns[3].Type);
				Assert.Equal("timestamp", columns[4].Type);
				Assert.Equal("float", columns[5].Type);
				Assert.Equal("double", columns[6].Type);
				Assert.Equal("varchar(100)", columns[7].Type);
				Assert.Equal("longtext", columns[8].Type);
				Assert.Equal("decimal(18,2)", columns[9].Type);
				Assert.Equal("timestamp", columns[10].Type);
				Assert.Equal("date", columns[11].Type);
				Assert.Equal("bigint(20)", columns[12].Type);

				conn.Execute("drop table `test`.`table15`");
			}
		}

		[Fact]
		public void SqlServerDataTypeTests()
		{
#if NETSTANDARD
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
#endif
			using (var conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"))
			{
				try
				{
					conn.Execute("create database test;");
				}
				catch
				{
				}
				try
				{
					conn.Execute("USE [test]; drop table [test].dbo.[table15]");
				}
				catch
				{
				}
				

				var spider = new DefaultSpider();

				EntityProcessor<Entity15> processor = new EntityProcessor<Entity15>();

				var pipeline = new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true");
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems.AddOrUpdateResultItem(processor.Model.Identity, new Tuple<IModel, IEnumerable<dynamic>>(processor.Model, new dynamic[] {
					new Dictionary<string, dynamic>
					{
						{ "int", "1"},
						{ "bool", "1"},
						{ "bigint", "11"},
						{ "string", "aaa"},
						{ "time", "2018-06-12"},
						{ "float", "1"},
						{ "double", "1"},
						{ "string1", "abc"},
						{ "string2", "abcdd"},
						{ "decimal", "1"}
					}
				}));
				pipeline.Process(new ResultItems[] { resultItems }, spider);

				var columns = conn.Query<ColumnInfo>("USE [test];select  b.name Name,c.name+'(' + cast(c.length as varchar)+')' [Type] from sysobjects a,syscolumns b,systypes c where a.id=b.id and a.name='table15' and a.xtype='U'and b.xtype=c.xtype").ToList();
				Assert.Equal(16, columns.Count);

				Assert.Equal("creation_date".ToLower(), columns[0].Name);
				Assert.Equal("int".ToLower(), columns[1].Name);
				Assert.Equal("time".ToLower(), columns[2].Name);
				Assert.Equal("creation_time".ToLower(), columns[3].Name);
				Assert.Equal("float".ToLower(), columns[4].Name);
				Assert.Equal("double".ToLower(), columns[5].Name);
				Assert.Equal("bool".ToLower(), columns[6].Name);
				Assert.Equal("decimal".ToLower(), columns[7].Name);
				Assert.Equal("bigint".ToLower(), columns[8].Name);
				Assert.Equal("id".ToLower(), columns[9].Name);
				Assert.Equal("string".ToLower(), columns[10].Name);
				Assert.Equal("string1".ToLower(), columns[11].Name);
				Assert.Equal("string2".ToLower(), columns[12].Name);


				Assert.Equal("date(3)", columns[0].Type);
				Assert.Equal("int(4)", columns[1].Type);
				Assert.Equal("datetime(8)", columns[2].Type);
				Assert.Equal("datetime(8)", columns[3].Type);
				Assert.Equal("float(8)", columns[4].Type);
				Assert.Equal("float(8)", columns[5].Type);
				Assert.Equal("bit(1)", columns[6].Type);
				Assert.Equal("decimal(17)", columns[7].Type);
				Assert.Equal("bigint(8)", columns[8].Type);
				Assert.Equal("bigint(8)", columns[9].Type);
				Assert.Equal("nvarchar(8000)", columns[10].Type);
				Assert.Equal("nvarchar(8000)", columns[11].Type);
				Assert.Equal("nvarchar(8000)", columns[12].Type);

				conn.Execute("USE [test]; drop table [test].dbo.[table15]");
			}
		}

		public class ColumnInfo
		{
			public string Name { get; set; }
			public string Type { get; set; }

			public override string ToString()
			{
				return $"{Name} {Type}";
			}
		}
	}
}
