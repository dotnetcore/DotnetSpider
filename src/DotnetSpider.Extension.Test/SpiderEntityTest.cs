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
using Xunit;

namespace DotnetSpider.Extension.Test
{
	public class SpiderEntityTest
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
		}

		[Schema("db", "table")]
		[Indexes(Primary = "name")]
		public class Entity1 : ISpiderEntity
		{
		}

		[Schema("db", "table")]
		[Indexes(Index = new[] { "c1" })]
		public class Entity2 : ISpiderEntity
		{
		}

		[Schema("db", "table")]
		[Indexes(Unique = new[] { "c1" })]
		public class Entity3 : ISpiderEntity
		{
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

		[Schema("db", "table")]
		public class Entity12 : ISpiderEntity
		{
			[PropertySelector(Expression = "Id")]
			public int Id { get; set; }

			[PropertySelector(Expression = "Name")]
			public string Name { get; set; }
		}

		public class Entity13 : ISpiderEntity
		{
			[PropertySelector(Expression = "Url")]
			public string Url { get; set; }
		}

		[Fact]
		public void EntitySelector()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity7).GetTypeInfo());
			Assert.Equal("expression", entity1.Entity.Selector.Expression);
			Assert.Equal(SelectorType.XPath, entity1.Entity.Selector.Type);
			Assert.True(entity1.Entity.Multi);

			var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity8).GetTypeInfo());
			Assert.Equal("expression2", entity2.Entity.Selector.Expression);
			Assert.Equal(SelectorType.Css, entity2.Entity.Selector.Type);
			Assert.True(entity2.Entity.Multi);

			var entity3 = EntitySpider.ParseEntityMetaData(typeof(Entity9).GetTypeInfo());
			Assert.False(entity3.Entity.Multi);
			Assert.Null(entity3.Entity.Selector);
			Assert.Equal("DotnetSpider.Extension.Test.SpiderEntityTest+Entity9", entity3.Entity.Name);
		}

		[Fact]
		public void Indexes()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity10).GetTypeInfo());
			Assert.Equal("Id", entity1.Indexes[0][0]);
			Assert.Equal("name", entity1.Primary[0]);
			Assert.Equal(2, entity1.Uniques.Count);
			Assert.Equal("Id", entity1.Uniques[0][0]);
			Assert.Equal("Name", entity1.Uniques[0][1]);
			Assert.Equal("Id", entity1.Uniques[1][0]);
		}

		[Fact]
		public void Formater()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity11).GetTypeInfo());
			var formatters = ((Field)entity1.Entity.Fields[1]).Formatters;
			Assert.Equal(2, formatters.Count);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.Equal("a", replaceFormatter.NewValue);
			Assert.Equal("b", replaceFormatter.OldValue);
		}

		[Fact]
		public void Schema()
		{
			var entityMetadata = EntitySpider.ParseEntityMetaData(typeof(Entity4).GetTypeInfo());
			Assert.Equal("db", entityMetadata.Schema.Database);
			Assert.Equal("table", entityMetadata.Schema.TableName);
			Assert.Equal(TableSuffix.Monday, entityMetadata.Schema.Suffix);

			var entityMetadata1 = EntitySpider.ParseEntityMetaData(typeof(Entity13).GetTypeInfo());
			Assert.Null(entityMetadata1.Schema);
		}

		[Fact]
		public void SetPrimary()
		{
			var entity1 = EntitySpider.ParseEntityMetaData(typeof(Entity5).GetTypeInfo());
			Assert.Equal(1, entity1.Entity.Fields.Count);
			Assert.Equal("Name", entity1.Entity.Fields[0].Name);
			var entity2 = EntitySpider.ParseEntityMetaData(typeof(Entity6).GetTypeInfo());
			Assert.Equal(1, entity2.Entity.Fields.Count);
			Assert.Equal("name", entity2.Entity.Fields[0].Name);
		}

		[Fact]
		public void SetNotExistColumnToPrimary()
		{
			var exception = Assert.Throws<SpiderException>(() =>
			{
				var entityMetadata = EntitySpider.ParseEntityMetaData(typeof(Entity1).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitiEntity(entityMetadata);
			});
			Assert.Equal("Columns set as primary is not a property of your entity.", exception.Message);
		}

		[Fact]
		public void SetNotExistColumnToIndex()
		{
			var exception = Assert.Throws<SpiderException>(() =>
			{
				var entityMetadata = EntitySpider.ParseEntityMetaData(typeof(Entity2).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitiEntity(entityMetadata);
			});
			Assert.Equal("Columns set as index is not a property of your entity.", exception.Message);
		}

		[Fact]
		public void SetNotExistColumnToUnique()
		{
			var exception = Assert.Throws<SpiderException>(() =>
			{
				var entityMetadata = EntitySpider.ParseEntityMetaData(typeof(Entity3).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitiEntity(entityMetadata);
			});
			Assert.Equal("Columns set as unique is not a property of your entity.", exception.Message);
		}

		[Fact]
		public void MultiEntitiesInitPipelines()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetThreadNum(1);
			context.SetIdentity("test-MultiEntitiesInitPipelines");
			context.AddEntityPipeline(new MySqlEntityPipeline("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			context.AddEntityPipeline(new MySqlFileEntityPipeline());
			context.AddEntityPipeline(new ConsoleEntityPipeline());
			context.AddEntityPipeline(new JsonFileEntityPipeline());
#if !NET_CORE
			//context.AddEntityPipeline(new MongoDbEntityPipeline("mongo"));
#endif
			context.AddStartUrl("http://a.com");
			context.AddEntityType(typeof(Entity13));
			context.AddEntityType(typeof(Entity12));
			context.Run("running-test");

			var entityPipelines = context.EntityPipelines;
#if NET_CORE
			Assert.Equal(4, entityPipelines.Count);
#else
			Assert.Equal(4, entityPipelines.Count);
			//Assert.Equal(5, entityPipelines.Count);
#endif
			var pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.Equal("Database='test';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306", pipeline1.ConnectString);

			Assert.Equal("MySqlFileEntityPipeline", entityPipelines[1].GetType().Name);
			Assert.Equal("ConsoleEntityPipeline", entityPipelines[2].GetType().Name);
			Assert.Equal("JsonFileEntityPipeline", entityPipelines[3].GetType().Name);
#if !NET_CORE
			//Assert.Equal("MongoDbEntityPipeline", entityPipelines[4].GetType().Name);
			//var pipeline2 = (MySqlEntityPipeline)entityPipelines[4];
			//Assert.Equal("mongo", pipeline2.ConnectString);
#endif
			var pipelines = context.GetPipelines();
			Assert.Equal(1, pipelines.Count);
			EntityPipeline pipeline = (EntityPipeline)pipelines[0];
			entityPipelines = pipeline.GetEntityPipelines();
			Assert.Equal(4, entityPipelines.Count);
			pipeline1 = (MySqlEntityPipeline)entityPipelines[0];
			Assert.Equal("db", pipeline1.GetSchema().Database);
			Assert.Equal("table", pipeline1.GetSchema().TableName);
#if !NET_CORE
			//var pipeline2 = (MongoDbEntityPipeline)entityPipelines[4];
			//Assert.Equal("db", pipeline2.GetSchema().Database);
			//Assert.Equal("table", pipeline2.GetSchema().TableName);
#endif

			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"))
			{
				conn.Execute($"DROP table db.table");
			}
		}

	}
}
