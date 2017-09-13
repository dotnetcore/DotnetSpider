using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace DotnetSpider.Extension.Test
{

	public class EntitySpiderTest2
	{
		public EntitySpiderTest2()
		{
		}

		private class TestPipeline : BaseEntityDbPipeline
		{
			public TestPipeline(string connectString, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
			{
			}

			protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
			{
				return new ConnectionStringSettings();
			}

			protected override DbParameter CreateDbParameter(string name, object value)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateCreateDatabaseSql(EntityAdapter metadata, string serverVersion)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateCreateTableSql(EntityAdapter metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateIfDatabaseExistsSql(EntityAdapter metadata, string serverVersion)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateInsertSql(EntityAdapter metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateSelectSql(EntityAdapter metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GenerateUpdateSql(EntityAdapter metadata)
			{
				throw new NotImplementedException();
			}
		}

		[EntityTable("test", "table", Primary = "name")]
		public class Entity1 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[EntityTable("test", "table", Indexs = new[] { "c1" })]
		public class Entity2 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[EntityTable("test", "table", Uniques = new[] { "c1" })]
		public class Entity3 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[EntityTable("test", "table", EntityTable.Monday)]
		public class Entity4 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table", Primary = "Name")]
		public class Entity5 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 100)]
			public string Name { get; set; }
		}

		[EntityTable("test", "table", Primary = "name")]
		public class Entity6 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 255)]
			public string name { get; set; }
		}

		[EntityTable("test", "table")]
		[EntitySelector(Expression = "expression")]
		public class Entity7 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table")]
		[EntitySelector(Expression = "expression2", Type = SelectorType.Css)]
		public class Entity8 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table")]
		public class Entity9 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table", Primary = "Name", Indexs = new[] { "Id" }, Uniques = new[] { "Id,Name", "Id" })]
		public class Entity10 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public int Id { get; set; }

			[PropertyDefine(Expression = "", Length = 100)]
			public string Name { get; set; }
		}

		[EntityTable("test", "table")]
		public class Entity11 : SpiderEntity
		{
			public int Id { get; set; }

			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			[PropertyDefine(Expression = "Name")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table12")]
		public class Entity12 : SpiderEntity
		{
			[PropertyDefine(Expression = "Id")]
			public int Id { get; set; }

			[PropertyDefine(Expression = "Name")]
			public string Name { get; set; }
		}

		[EntityTable("test", "table13")]
		public class Entity13 : SpiderEntity
		{
			[PropertyDefine(Expression = "Url")]
			public string Url { get; set; }
		}

		public class Entity14 : SpiderEntity
		{
			[PropertyDefine(Expression = "Url")]
			public string Url { get; set; }
		}

		[EntityTable("test", "table15")]
		public class Entity15 : SpiderEntity
		{
			[PropertyDefine(Expression = "Url")]
			public int Int { get; set; }

			[PropertyDefine(Expression = "Url")]
			public long BigInt { get; set; }

			[PropertyDefine(Expression = "Url")]
			public string String { get; set; }

			[PropertyDefine(Expression = "Url")]
			public DateTime Time { get; set; }

			[PropertyDefine(Expression = "Url")]
			public float Float { get; set; }

			[PropertyDefine(Expression = "Url")]
			public double Double { get; set; }

			[PropertyDefine(Expression = "Url", Length = 100)]
			public string String1 { get; set; }
		}

		[EntityTable("test", "table", Indexs = new[] { "c1" })]
		public class Entity16 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public int c1 { get; set; }
		}

		[EntityTable("test", "table", Indexs = new[] { "c1" })]
		public class Entity17 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[EntityTable("test", "table", Uniques = new[] { "c1" })]
		public class Entity18 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[EntityTable("test", "table", Primary = "c1")]
		public class Entity19 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[Fact]
		public void EntitySelector()
		{
			var entity1 = EntityDefine.Parse<Entity7>();
			Assert.Equal("expression", entity1.Selector.Expression);
			Assert.Equal(SelectorType.XPath, entity1.Selector.Type);
			Assert.True(entity1.Multi);

			var entity2 = EntityDefine.Parse<Entity8>();
			Assert.Equal("expression2", entity2.Selector.Expression);
			Assert.Equal(SelectorType.Css, entity2.Selector.Type);
			Assert.True(entity2.Multi);

			var entity3 = EntityDefine.Parse<Entity9>();
			Assert.False(entity3.Multi);
			Assert.Null(entity3.Selector);
			Assert.Equal("DotnetSpider.Extension.Test.EntitySpiderTest2+Entity9", entity3.Name);
		}

		[Fact]
		public void Indexes()
		{
			var entity1 = EntityDefine.Parse<Entity10>();
			Assert.Equal("Id", entity1.TableInfo.Indexs[0]);
			Assert.Equal("Name", entity1.TableInfo.Primary);
			Assert.Equal(2, entity1.TableInfo.Uniques.Length);
			Assert.Equal("Id,Name", entity1.TableInfo.Uniques[0]);
			Assert.Equal("Id", entity1.TableInfo.Uniques[1]);
		}

		[Fact]
		public void ColumnOfIndexesOverLength()
		{
			try
			{
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;

				var entity = context.AddEntityType<Entity17>();
				var pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306");
				pipeline.AddEntity(entity);

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of index should not large than 256.", e.Message);
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

				var entity = context.AddEntityType<Entity18>();
				var pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306");
				pipeline.AddEntity(entity);

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of unique should not large than 256.", e.Message);
			}
		}

		[Fact]
		public void ColumnOfPrimayOverLength()
		{
			try
			{
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;

				var entity = context.AddEntityType<Entity19>();
				var pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306");
				pipeline.AddEntity(entity);

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of primary should not large than 256.", e.Message);
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
			var entity1 = EntityDefine.Parse<Entity11>();
			var formatters = ((Column)entity1.Columns[0]).Formatters;
			Assert.Equal(2, formatters.Count);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.Equal("a", replaceFormatter.NewValue);
			Assert.Equal("b", replaceFormatter.OldValue);
		}

		[Fact]
		public void Schema()
		{
			var entityMetadata = EntityDefine.Parse<Entity4>();
			Assert.Equal("test", entityMetadata.TableInfo.Database);
			Assert.Equal(EntityTable.Monday, entityMetadata.TableInfo.Postfix);

			var entityMetadata1 = EntityDefine.Parse<Entity14>();
			Assert.Null(entityMetadata1.TableInfo);
		}

		[Fact]
		public void SetPrimary()
		{
			var entity1 = EntityDefine.Parse<Entity5>();
			Assert.Single(entity1.Columns);
			Assert.Equal("Name", entity1.Columns[0].Name);
			var entity2 = EntityDefine.Parse<Entity6>();
			Assert.Single(entity2.Columns);
			Assert.Equal("name", entity2.Columns[0].Name);
		}

		[Fact]
		public void SetNotExistColumnToPrimary()
		{
			try
			{
				var entityMetadata = EntityDefine.Parse<Entity1>();
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Columns set as primary is not a property of your entity.", exception.Message);
			}
		}

		[Fact]
		public void SetNotExistColumnToIndex()
		{
			try
			{
				var entityMetadata = EntityDefine.Parse<Entity2>();
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Columns set as index is not a property of your entity.", exception.Message);
			}
		}

		[Fact]
		public void SetNotExistColumnToUnique()
		{
			try
			{
				var entityMetadata = EntityDefine.Parse<Entity3>();
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.Equal("Columns set as unique is not a property of your entity.", exception.Message);
			}
		}
		[Fact]
		public void MySqlFileEntityPipeline_InsertSql()
		{
			var id = Guid.NewGuid().ToString("N");
			var folder = Path.Combine(Core.Environment.BaseDirectory, id);
			var path = Path.Combine(folder, "mysql", "baidu.baidu_search_mysql_file.sql");
			try
			{
				BaiduSearchSpider spider = new BaiduSearchSpider();
				spider.Identity = id;
				spider.Run();

				var lines = File.ReadAllLines(path);
				Assert.Equal(20, lines.Length);
				using (var conn = new MySqlConnection(Core.Environment.DataConnectionStringSettings.ConnectionString))
				{
					conn.Execute("DELETE FROM baidu.baidu_search_mysql_file");
					foreach (var sql in lines)
					{
						conn.Execute(sql);
					}
					var count = conn.QueryFirst<int>("SELECT COUNT(*) FROM baidu.baidu_search_mysql_file");
					Assert.Equal(20, count);
					conn.Execute("DROP TABLE baidu.baidu_search_mysql_file");
				}
			}
			finally
			{
				if (Directory.Exists(folder))
				{
					Directory.Delete(folder, true);
				}
			}
		}

		class BaiduSearchSpider : EntitySpider
		{
			public BaiduSearchSpider() : base("BaiduSearch")
			{
			}

			protected override void MyInit(params string[] arguments)
			{
				EmptySleepTime = 1000;
				var word = "可乐|雪碧";
				AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
				AddEntityType(typeof(BaiduSearchEntry));
				AddPipeline(new MySqlEntityPipeline(Core.Environment.DataConnectionStringSettings.ConnectionString));
				AddPipeline(new MySqlFileEntityPipeline(MySqlFileEntityPipeline.FileType.InsertSql));
			}

			[EntityTable("baidu", "baidu_search_mysql_file")]
			[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
			class BaiduSearchEntry : SpiderEntity
			{
				[PropertyDefine(Expression = "Keyword", Type = SelectorType.Enviroment)]
				public string Keyword { get; set; }

				[PropertyDefine(Expression = ".//h3[@class='c-title']/a")]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				public string Title { get; set; }

				[PropertyDefine(Expression = ".//h3[@class='c-title']/a/@href")]
				public string Url { get; set; }

				[PropertyDefine(Expression = ".//div/p[@class='c-author']/text()")]
				[ReplaceFormatter(NewValue = "-", OldValue = "&nbsp;")]
				public string Website { get; set; }


				[PropertyDefine(Expression = ".//div/span/a[@class='c-cache']/@href")]
				public string Snapshot { get; set; }


				[PropertyDefine(Expression = ".//div[@class='c-summary c-row ']", Option = PropertyDefine.Options.PlainText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string Details { get; set; }

				[PropertyDefine(Expression = ".", Option = PropertyDefine.Options.PlainText)]
				[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
				[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
				[ReplaceFormatter(NewValue = " ", OldValue = "&nbsp;")]
				public string PlainText { get; set; }

				[PropertyDefine(Expression = "today", Type = SelectorType.Enviroment)]
				public DateTime run_id { get; set; }
			}
		}

		[Fact]
		public void MultiEntitiesInitPipelines()
		{
			EntitySpider context = new DefaultEntitySpider();
			context.Identity = (Guid.NewGuid().ToString("N"));
			context.ThreadNum = 1;
			context.AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));
			context.AddPipeline(new MySqlFileEntityPipeline());
			context.AddPipeline(new ConsoleEntityPipeline());
			context.AddPipeline(new JsonFileEntityPipeline());

			context.AddStartUrl("http://baidu.com");
			context.AddEntityType(typeof(Entity13));
			context.AddEntityType(typeof(Entity12));
			context.Run("running-test");

			var entityPipelines = context.ReadOnlyPipelines;

			Assert.Equal(4, entityPipelines.Count);

			var pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;", pipeline1.ConnectionStringSettings.ConnectionString);

			Assert.Equal("MySqlFileEntityPipeline", entityPipelines[1].GetType().Name);
			Assert.Equal("ConsoleEntityPipeline", entityPipelines[2].GetType().Name);
			Assert.Equal("JsonFileEntityPipeline", entityPipelines[3].GetType().Name);

			var pipelines = context.GetPipelines();
			Assert.Equal(4, pipelines.Count);
			IEntityPipeline pipeline = (IEntityPipeline)pipelines[0];
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
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;
				context.AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));

				context.AddStartUrl("http://baidu.com");
				context.AddEntityType(typeof(Entity15));

				context.Run("running-test");


				var columns = conn.Query<ColumnInfo>("SELECT COLUMN_NAME as `Name`, COLUMN_TYPE as `Type` FROM information_schema.columns WHERE table_name='table15' AND table_schema = 'test';").ToList(); ;
				Assert.Equal(9, columns.Count);

				Assert.Equal("Int", columns[0].Name);
				Assert.Equal("BigInt", columns[1].Name);
				Assert.Equal("String", columns[2].Name);
				Assert.Equal("Time", columns[3].Name);
				Assert.Equal("Float", columns[4].Name);
				Assert.Equal("Double", columns[5].Name);
				Assert.Equal("String1", columns[6].Name);
				Assert.Equal("CDate", columns[7].Name);
				Assert.Equal(Core.Environment.IdColumn, columns[8].Name);

				Assert.Equal("int(11)", columns[0].Type);
				Assert.Equal("bigint(20)", columns[1].Type);
				Assert.Equal("text", columns[2].Type);
				Assert.Equal("timestamp", columns[3].Type);
				Assert.Equal("float", columns[4].Type);
				Assert.Equal("double", columns[5].Type);
				Assert.Equal("varchar(100)", columns[6].Type);
				Assert.Equal("timestamp", columns[7].Type);
				Assert.Equal("bigint(20)", columns[8].Type);

				conn.Execute("drop table `test`.`table15`");
			}
		}

		[Fact]
		public void SqlServerDataTypeTests()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}
			using (var conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"))
			{
				try
				{
					conn.Execute("create database test;");
				}
				catch
				{
				}
				EntitySpider context = new DefaultEntitySpider();
				context.Identity = (Guid.NewGuid().ToString("N"));
				context.ThreadNum = 1;
				context.AddPipeline(new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"));

				context.AddStartUrl("http://baidu.com");
				context.AddEntityType(typeof(Entity15));

				context.Run("running-test");


				var columns = conn.Query<ColumnInfo>("USE [test];select  b.name Name,c.name+'(' + cast(c.length as varchar)+')' [Type] from sysobjects a,syscolumns b,systypes c where a.id=b.id and a.name='table15' and a.xtype='U'and b.xtype=c.xtype").ToList();
				Assert.Equal(11, columns.Count);

				Assert.Equal("Int", columns[0].Name);
				Assert.Equal("Time", columns[1].Name);
				Assert.Equal("CDate", columns[2].Name);
				Assert.Equal("Float", columns[3].Name);
				Assert.Equal("Double", columns[4].Name);
				Assert.Equal("BigInt", columns[5].Name);
				Assert.Equal(Core.Environment.IdColumn, columns[6].Name);
				Assert.Equal("String", columns[7].Name);
				Assert.Equal("String1", columns[8].Name);

				Assert.Equal("int(4)", columns[0].Type);
				Assert.Equal("datetime(8)", columns[1].Type);
				Assert.Equal("datetime(8)", columns[2].Type);
				Assert.Equal("float(8)", columns[3].Type);
				Assert.Equal("float(8)", columns[4].Type);
				Assert.Equal("bigint(8)", columns[5].Type);
				Assert.Equal("bigint(8)", columns[6].Type);
				Assert.Equal("nvarchar(8000)", columns[7].Type);
				Assert.Equal("nvarchar(8000)", columns[8].Type);

				conn.Execute("USE [test]; drop table [test].dbo.[table15]");
			}
		}

		public class ColumnInfo
		{
			public string Name { get; set; }
			public string Type { get; set; }
		}
	}
}
