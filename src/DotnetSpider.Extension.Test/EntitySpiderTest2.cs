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
using DotnetSpider.Extension.Processor;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class EntitySpiderTest2
	{
		private class TestPipeline : BaseEntityDbPipeline
		{
			protected override DbConnection CreateConnection()
			{
				throw new NotImplementedException();
			}

			protected override string GetInsertSql()
			{
				throw new NotImplementedException();
			}

			protected override string GetUpdateSql()
			{
				throw new NotImplementedException();
			}

			protected override string GetSelectSql()
			{
				throw new NotImplementedException();
			}

			protected override string GetCreateTableSql()
			{
				throw new NotImplementedException();
			}

			protected override string GetCreateSchemaSql()
			{
				throw new NotImplementedException();
			}

			protected override DbParameter CreateDbParameter(string name, object value)
			{
				throw new NotImplementedException();
			}

			protected override string ConvertToDbType(string datatype)
			{
				throw new NotImplementedException();
			}

			public override BaseEntityPipeline Clone()
			{
				return new TestPipeline();
			}

			protected override string GetIfSchemaExistsSql()
			{
				throw new NotImplementedException();
			}
		}

		[Schema("db", "table")]
		[Indexes(Primary = "name")]
		public class Entity1 : ISpiderEntity
		{
			[StoredAs("url", DataType.String)]
			public string Url { get; set; }
		}

		[Schema("db", "table")]
		[Indexes(Index = new[] { "c1" })]
		public class Entity2 : ISpiderEntity
		{
			[StoredAs("url", DataType.String)]
			public string Url { get; set; }
		}

		[Schema("db", "table")]
		[Indexes(Unique = new[] { "c1" })]
		public class Entity3 : ISpiderEntity
		{
			[StoredAs("url", DataType.String)]
			public string Url { get; set; }
		}

		[Schema("db", "table", TableSuffix.Monday)]
		public class Entity4 : ISpiderEntity
		{
			public string Name { get; set; }
		}

		[Indexes(Primary = "Name")]
		[Schema("db", "table")]
		public class Entity5 : ISpiderEntity
		{
			public string Name { get; set; }
		}

		[Indexes(Primary = "name")]
		[Schema("db", "table")]
		public class Entity6 : ISpiderEntity
		{
			[StoredAs("name", DataType.String, 255)]
			public string Name { get; set; }
		}

		[Schema("db", "table")]
		[EntitySelector(Expression = "expression")]
		public class Entity7 : ISpiderEntity
		{
			public string Name { get; set; }
		}

		[Schema("db", "table")]
		[EntitySelector(Expression = "expression2", Type = SelectorType.Css)]
		public class Entity8 : ISpiderEntity
		{
			public string Name { get; set; }
		}

		[Schema("db", "table")]
		public class Entity9 : ISpiderEntity
		{
			public string Name { get; set; }
		}

		[Schema("db", "table")]
		[Indexes(Primary = "name", AutoIncrement = new[] { "Id" }, Index = new[] { "Id" }, Unique = new[] { "Id,Name", "Id" })]
		public class Entity10 : ISpiderEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[Schema("db", "table")]
		public class Entity11 : ISpiderEntity
		{
			public int Id { get; set; }
			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			public string Name { get; set; }
		}

		[Schema("db", "table12")]
		public class Entity12 : ISpiderEntity
		{
			[PropertySelector(Expression = "Id")]
			public int Id { get; set; }
			[StoredAs("url", DataType.String)]
			[PropertySelector(Expression = "Name")]
			public string Name { get; set; }
		}

		[Schema("db", "table13")]
		public class Entity13 : ISpiderEntity
		{
			[PropertySelector(Expression = "Url")]
			[StoredAs("url", DataType.String)]
			public string Url { get; set; }
		}

		public class Entity14 : ISpiderEntity
		{
			[PropertySelector(Expression = "Url")]
			[StoredAs("url", DataType.String)]
			public string Url { get; set; }
		}

		[TestMethod]
		public void EntitySelector()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity7).GetTypeInfo());
			Assert.AreEqual("expression", entity1.Entity.Selector.Expression);
			Assert.AreEqual(SelectorType.XPath, entity1.Entity.Selector.Type);
			Assert.IsTrue(entity1.Entity.Multi);

			var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity8).GetTypeInfo());
			Assert.AreEqual("expression2", entity2.Entity.Selector.Expression);
			Assert.AreEqual(SelectorType.Css, entity2.Entity.Selector.Type);
			Assert.IsTrue(entity2.Entity.Multi);

			var entity3 = EntitySpider.GenerateEntityMetaData(typeof(Entity9).GetTypeInfo());
			Assert.IsFalse(entity3.Entity.Multi);
			Assert.IsNull(entity3.Entity.Selector);
			Assert.AreEqual("DotnetSpider.Extension.Test.EntitySpiderTest2+Entity9", entity3.Entity.Name);
		}

		[TestMethod]
		public void Indexes()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity10).GetTypeInfo());
			Assert.AreEqual("Id", entity1.Indexes[0][0]);
			Assert.AreEqual("name", entity1.Primary[0]);
			Assert.AreEqual(2, entity1.Uniques.Count);
			Assert.AreEqual("Id", entity1.Uniques[0][0]);
			Assert.AreEqual("Name", entity1.Uniques[0][1]);
			Assert.AreEqual("Id", entity1.Uniques[1][0]);
		}

		[TestMethod]
		public void Formater()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity11).GetTypeInfo());
			var formatters = ((Field)entity1.Entity.Fields[1]).Formatters;
			Assert.AreEqual(2, formatters.Count);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.AreEqual("a", replaceFormatter.NewValue);
			Assert.AreEqual("b", replaceFormatter.OldValue);
		}

		[TestMethod]
		public void Schema()
		{
			var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity4).GetTypeInfo());
			Assert.AreEqual("db", entityMetadata.Schema.Database);
			Assert.AreEqual("table", entityMetadata.Schema.TableName);
			Assert.AreEqual(TableSuffix.Monday, entityMetadata.Schema.Suffix);

			var entityMetadata1 = EntitySpider.GenerateEntityMetaData(typeof(Entity14).GetTypeInfo());
			Assert.IsNull(entityMetadata1.Schema);
		}

		[TestMethod]
		public void SetPrimary()
		{
			var entity1 = EntitySpider.GenerateEntityMetaData(typeof(Entity5).GetTypeInfo());
			Assert.AreEqual(1, entity1.Entity.Fields.Count);
			Assert.AreEqual("Name", entity1.Entity.Fields[0].Name);
			var entity2 = EntitySpider.GenerateEntityMetaData(typeof(Entity6).GetTypeInfo());
			Assert.AreEqual(1, entity2.Entity.Fields.Count);
			Assert.AreEqual("name", entity2.Entity.Fields[0].Name);
		}

		[TestMethod]
		public void SetNotExistColumnToPrimary()
		{
			try
			{
				var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity1).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitEntity(entityMetadata);
				throw new Exception("Test failed");
			}
			catch (SpiderException exception)
			{
				Assert.AreEqual("Columns set as primary is not a property of your entity.", exception.Message);
			}
		}

		[TestMethod]
		public void SetNotExistColumnToIndex()
		{
			try
			{
				var entityMetadata = EntitySpider.GenerateEntityMetaData(typeof(Entity2).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitEntity(entityMetadata);
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
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitEntity(entityMetadata);
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
			context.AddEntityPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddEntityPipeline(new MySqlFileEntityPipeline());
			context.AddEntityPipeline(new ConsoleEntityPipeline());
			context.AddEntityPipeline(new JsonFileEntityPipeline());

			context.AddStartUrl("http://baidu.com");
			context.AddEntityType(typeof(Entity13));
			context.AddEntityType(typeof(Entity12));
			context.Run("running-test");

			var entityPipelines = context.EntityPipelines;

			Assert.AreEqual(4, entityPipelines.Count);

			var pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.AreEqual("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306", pipeline1.ConnectString);

			Assert.AreEqual("MySqlFileEntityPipeline", entityPipelines[1].GetType().Name);
			Assert.AreEqual("ConsoleEntityPipeline", entityPipelines[2].GetType().Name);
			Assert.AreEqual("JsonFileEntityPipeline", entityPipelines[3].GetType().Name);

			var pipelines = context.GetPipelines();
			Assert.AreEqual(2, pipelines.Count);
			EntityPipeline pipeline = (EntityPipeline)pipelines[0];
			entityPipelines = pipeline.GetEntityPipelines();
			Assert.AreEqual(4, entityPipelines.Count);
			pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.AreEqual("db", pipeline1.GetSchema().Database);
			Assert.AreEqual("table13", pipeline1.GetSchema().TableName);

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				conn.Execute($"DROP table db.table12");
				conn.Execute($"DROP table db.table13");
			}
		}

	}
}
