using System;
using System.Data.Common;
using System.Reflection;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Data.SqlClient;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class EntitySpiderTest2
	{
		private class TestPipeline : BaseEntityDbPipeline
		{
			public TestPipeline(string connectString, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
			{
			}

			protected override DbConnection CreateConnection()
			{
				throw new NotImplementedException();
			}

			protected override DbParameter CreateDbParameter(string name, object value)
			{
				throw new NotImplementedException();
			}

			protected override string GetCreateSchemaSql(EntityDbMetadata metadata, string serverVersion)
			{
				throw new NotImplementedException();
			}

			protected override string GetCreateTableSql(EntityDbMetadata metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GetIfSchemaExistsSql(EntityDbMetadata metadata, string serverVersion)
			{
				throw new NotImplementedException();
			}

			protected override string GetInsertSql(EntityDbMetadata metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GetSelectSql(EntityDbMetadata metadata)
			{
				throw new NotImplementedException();
			}

			protected override string GetUpdateSql(EntityDbMetadata metadata)
			{
				throw new NotImplementedException();
			}
		}

		[Table("test", "table", Primary = "name")]
		public class Entity1 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[Table("test", "table", Indexs = new[] { "c1" })]
		public class Entity2 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[Table("test", "table", Uniques = new[] { "c1" })]
		public class Entity3 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Url { get; set; }
		}

		[Table("test", "table", TableSuffix.Monday)]
		public class Entity4 : SpiderEntity
		{
			public string Name { get; set; }
		}

		[Table("test", "table", Primary = "Name")]
		public class Entity5 : SpiderEntity
		{
			[PropertyDefine(Expression = "")]
			public string Name { get; set; }
		}

		[Table("test", "table", Primary = "name")]
		public class Entity6 : SpiderEntity
		{
			[PropertyDefine(Expression = "", Length = 255)]
			public string name { get; set; }
		}

		[Table("test", "table")]
		[EntitySelector(Expression = "expression")]
		public class Entity7 : SpiderEntity
		{
			public string Name { get; set; }
		}

		[Table("test", "table")]
		[EntitySelector(Expression = "expression2", Type = SelectorType.Css)]
		public class Entity8 : SpiderEntity
		{
			public string Name { get; set; }
		}

		[Table("test", "table")]
		public class Entity9 : SpiderEntity
		{
			public string Name { get; set; }
		}

		[Table("test", "table", Primary = "Name", Indexs = new[] { "Id" }, Uniques = new[] { "Id,Name", "Id" })]
		public class Entity10 : SpiderEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[Table("test", "table")]
		public class Entity11 : SpiderEntity
		{
			public int Id { get; set; }

			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			[PropertyDefine(Expression = "Name")]
			public string Name { get; set; }
		}

		[Table("test", "table12")]
		public class Entity12 : SpiderEntity
		{
			[PropertyDefine(Expression = "Id")]
			public int Id { get; set; }

			[PropertyDefine(Expression = "Name")]
			public string Name { get; set; }
		}

		[Table("test", "table13")]
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

		[Table("test", "table15")]
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

		[TestMethod]
		public void EntitySelector()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity7).GetTypeInfo());
			Assert.AreEqual("expression", entity1.Selector.Expression);
			Assert.AreEqual(SelectorType.XPath, entity1.Selector.Type);
			Assert.IsTrue(entity1.Multi);

			var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity8).GetTypeInfo());
			Assert.AreEqual("expression2", entity2.Selector.Expression);
			Assert.AreEqual(SelectorType.Css, entity2.Selector.Type);
			Assert.IsTrue(entity2.Multi);

			var entity3 = EntitySpider.GenerateEntityMetaData(typeof(Entity9).GetTypeInfo());
			Assert.IsFalse(entity3.Multi);
			Assert.IsNull(entity3.Selector);
			Assert.AreEqual("DotnetSpider.Extension.Test.EntitySpiderTest2+Entity9", entity3.Name);
		}

		[TestMethod]
		public void Indexes()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity10).GetTypeInfo());
			Assert.AreEqual("Id", entity1.Table.Indexs[0]);
			Assert.AreEqual("Name", entity1.Table.Primary);
			Assert.AreEqual(2, entity1.Table.Uniques.Length);
			Assert.AreEqual("Id,Name", entity1.Table.Uniques[0]);
			Assert.AreEqual("Id", entity1.Table.Uniques[1]);
		}

		[TestMethod]
		public void Formater()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity11).GetTypeInfo());
			var formatters = ((Field)entity1.Fields[0]).Formatters;
			Assert.AreEqual(2, formatters.Count);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.AreEqual("a", replaceFormatter.NewValue);
			Assert.AreEqual("b", replaceFormatter.OldValue);
		}

		[TestMethod]
		public void Schema()
		{
			var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity4).GetTypeInfo());
			Assert.AreEqual("test", entityMetadata.Table.Database);
			Assert.AreEqual(EntitySpider.GenerateTableName("table", entityMetadata.Table.Suffix), entityMetadata.Table.Name);
			Assert.AreEqual(TableSuffix.Monday, entityMetadata.Table.Suffix);

			var entityMetadata1 = EntitySpider.GenerateEntityMetaData(typeof(Entity14).GetTypeInfo());
			Assert.IsNull(entityMetadata1.Table);
		}

		[TestMethod]
		public void SetPrimary()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity5).GetTypeInfo());
			Assert.AreEqual(1, entity1.Fields.Count);
			Assert.AreEqual("Name", entity1.Fields[0].Name);
			var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity6).GetTypeInfo());
			Assert.AreEqual(1, entity2.Fields.Count);
			Assert.AreEqual("name", entity2.Fields[0].Name);
		}

		[TestMethod]
		public void SetNotExistColumnToPrimary()
		{
			try
			{
				var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity1).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Columns set as Primary is not a property of your entity.", exception.Message);
			}
		}

		[TestMethod]
		public void SetNotExistColumnToIndex()
		{
			try
			{
				var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity2).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Columns set as index is not a property of your entity.", exception.Message);
			}
		}

		[TestMethod]
		public void SetNotExistColumnToUnique()
		{
			try
			{
				var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity3).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline("");
				pipeline.AddEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Columns set as unique is not a property of your entity.", exception.Message);
			}
		}

		[TestMethod]
		public void MultiEntitiesInitPipelines()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetIdentity(Guid.NewGuid().ToString("N"));
			context.SetThreadNum(1);
			context.AddPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddPipeline(new MySqlFileEntityPipeline());
			context.AddPipeline(new ConsoleEntityPipeline());
			context.AddPipeline(new JsonFileEntityPipeline());

			context.AddStartUrl("http://baidu.com");
			context.AddEntityType(typeof(Entity13));
			context.AddEntityType(typeof(Entity12));
			context.Run("running-test");

			var entityPipelines = context.Pipelines;

			Assert.AreEqual(4, entityPipelines.Count);

			var pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.AreEqual("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306", pipeline1.ConnectString);

			Assert.AreEqual("MySqlFileEntityPipeline", entityPipelines[1].GetType().Name);
			Assert.AreEqual("ConsoleEntityPipeline", entityPipelines[2].GetType().Name);
			Assert.AreEqual("JsonFileEntityPipeline", entityPipelines[3].GetType().Name);

			var pipelines = context.GetPipelines();
			Assert.AreEqual(4, pipelines.Count);
			IEntityPipeline pipeline = (IEntityPipeline)pipelines[0];
			//entityPipelines = pipeline.GetEntityPipelines();
			//Assert.AreEqual(4, entityPipelines.Count);
			//pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			//Assert.AreEqual("test", pipeline1.GetSchema().Database);
			//Assert.AreEqual("table13", pipeline1.GetSchema().Name);

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				conn.Execute($"DROP table test.table12");
				conn.Execute($"DROP table test.table13");
			}
		}


		[TestMethod]
		public void MySqlDataTypeTests()
		{
			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				EntitySpider context = new EntitySpider(new Site());
				context.SetIdentity(Guid.NewGuid().ToString("N"));
				context.SetThreadNum(1);
				context.AddPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));

				context.AddStartUrl("http://baidu.com");
				context.AddEntityType(typeof(Entity15));

				context.Run("running-test");


				var columns = conn.Query<ColumnInfo>("SELECT COLUMN_NAME as `Name`, COLUMN_TYPE as `Type` FROM information_schema.columns WHERE table_name='table15' AND table_schema = 'test';").ToList(); ;
				Assert.AreEqual(9, columns.Count);

				Assert.AreEqual("Int", columns[0].Name);
				Assert.AreEqual("BigInt", columns[1].Name);
				Assert.AreEqual("String", columns[2].Name);
				Assert.AreEqual("Time", columns[3].Name);
				Assert.AreEqual("Float", columns[4].Name);
				Assert.AreEqual("Double", columns[5].Name);
				Assert.AreEqual("String1", columns[6].Name);
				Assert.AreEqual("cdate", columns[7].Name);
				Assert.AreEqual("__id", columns[8].Name);

				Assert.AreEqual("int(11)", columns[0].Type);
				Assert.AreEqual("bigint(20)", columns[1].Type);
				Assert.AreEqual("text", columns[2].Type);
				Assert.AreEqual("timestamp", columns[3].Type);
				Assert.AreEqual("float", columns[4].Type);
				Assert.AreEqual("double", columns[5].Type);
				Assert.AreEqual("varchar(100)", columns[6].Type);
				Assert.AreEqual("timestamp", columns[7].Type);
				Assert.AreEqual("bigint(20)", columns[8].Type);

				conn.Execute("drop table `test`.`table15`");
			}
		}

		[TestMethod]
		public void SqlServerDataTypeTests()
		{
			using (var conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=test;Trusted_Connection=True;MultipleActiveResultSets=true"))
			{
				EntitySpider context = new EntitySpider(new Site());
				context.SetIdentity(Guid.NewGuid().ToString("N"));
				context.SetThreadNum(1);
				context.AddPipeline(new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=test;Trusted_Connection=True;MultipleActiveResultSets=true"));

				context.AddStartUrl("http://baidu.com");
				context.AddEntityType(typeof(Entity15));

				context.Run("running-test");


				var columns = conn.Query<ColumnInfo>("USE [test];select  b.name Name,c.name+'(' + cast(c.length as varchar)+')' [Type] from sysobjects a,syscolumns b,systypes c where a.id=b.id and a.name='table15' and a.xtype='U'and b.xtype=c.xtype").ToList(); ;
				Assert.AreEqual(11, columns.Count);

				Assert.AreEqual("Int", columns[0].Name);
				Assert.AreEqual("Time", columns[1].Name);
				Assert.AreEqual("CDate", columns[2].Name);
				Assert.AreEqual("Float", columns[3].Name);
				Assert.AreEqual("Double", columns[4].Name);
				Assert.AreEqual("BigInt", columns[5].Name);
				Assert.AreEqual("__Id", columns[6].Name);
				Assert.AreEqual("String", columns[7].Name);
				Assert.AreEqual("String1", columns[8].Name);

				Assert.AreEqual("int(4)", columns[0].Type);
				Assert.AreEqual("datetime(8)", columns[1].Type);
				Assert.AreEqual("datetime(8)", columns[2].Type);
				Assert.AreEqual("float(8)", columns[3].Type);
				Assert.AreEqual("float(8)", columns[4].Type);
				Assert.AreEqual("bigint(8)", columns[5].Type);
				Assert.AreEqual("bigint(8)", columns[6].Type);
				Assert.AreEqual("nvarchar(8000)", columns[7].Type);
				Assert.AreEqual("nvarchar(8000)", columns[8].Type);

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
