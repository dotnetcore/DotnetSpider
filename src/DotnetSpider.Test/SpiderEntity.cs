using System.Data.Common;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using Xunit;

namespace DotnetSpider.Test
{
	public class SpiderEntity
	{
		private class TestPipeline : BaseEntityDbPipeline
		{

			protected override DbConnection CreateConnection()
			{
				throw new System.NotImplementedException();
			}

			protected override string GetInsertSql()
			{
				throw new System.NotImplementedException();
			}

			protected override string GetUpdateSql()
			{
				throw new System.NotImplementedException();
			}

			protected override string GetCreateTableSql()
			{
				throw new System.NotImplementedException();
			}

			protected override string GetCreateSchemaSql()
			{
				throw new System.NotImplementedException();
			}

			protected override DbParameter CreateDbParameter()
			{
				throw new System.NotImplementedException();
			}

			protected override string ConvertToDbType(string datatype)
			{
				throw new System.NotImplementedException();
			}

			public override object Clone()
			{
				throw new System.NotImplementedException();
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
		[Indexes(Primary = "name", AutoIncrement = "Id", Index = new[] { "Id" }, Unique = new[] { "Id,Name", "Id" })]
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
			public string Name { get; set; }
		}

		[Fact]
		public void EntitySelector()
		{
			var entity1 = EntitySpider.PaserEntityMetaData(typeof(Entity7).GetTypeInfo());
			Assert.Equal("expression", entity1.Entity.Selector.Expression);
			Assert.Equal(SelectorType.XPath, entity1.Entity.Selector.Type);
			Assert.True(entity1.Entity.Multi);

			var entity2 = EntitySpider.PaserEntityMetaData(typeof(Entity8).GetTypeInfo());
			Assert.Equal("expression2", entity2.Entity.Selector.Expression);
			Assert.Equal(SelectorType.Css, entity2.Entity.Selector.Type);
			Assert.True(entity2.Entity.Multi);

			var entity3 = EntitySpider.PaserEntityMetaData(typeof(Entity9).GetTypeInfo());
			Assert.False(entity3.Entity.Multi);
			Assert.Null(entity3.Entity.Selector);
			Assert.Equal("DotnetSpider.Test.SpiderEntity+Entity9", entity3.Entity.Name);
		}

		[Fact]
		public void Indexes()
		{
			var entity1 = EntitySpider.PaserEntityMetaData(typeof(Entity10).GetTypeInfo());
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
			var entity1 = EntitySpider.PaserEntityMetaData(typeof(Entity11).GetTypeInfo());
			var formatters = ((Field)entity1.Entity.Fields[1]).Formatters;
			Assert.Equal(1, formatters.Count);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.Equal("a", replaceFormatter.NewValue);
			Assert.Equal("b", replaceFormatter.OldValue);
		}

		[Fact]
		public void Schema()
		{
			var entityMetadata = EntitySpider.PaserEntityMetaData(typeof(Entity4).GetTypeInfo());
			Assert.Equal("db", entityMetadata.Schema.Database);
			Assert.Equal("table", entityMetadata.Schema.TableName);
			Assert.Equal(TableSuffix.Monday, entityMetadata.Schema.Suffix);
		}

		[Fact]
		public void SetPrimary()
		{
			var entity1 = EntitySpider.PaserEntityMetaData(typeof(Entity5).GetTypeInfo());
			Assert.Equal(1, entity1.Entity.Fields.Count);
			Assert.Equal("Name", entity1.Entity.Fields[0].Name);
			var entity2 = EntitySpider.PaserEntityMetaData(typeof(Entity6).GetTypeInfo());
			Assert.Equal(1, entity2.Entity.Fields.Count);
			Assert.Equal("name", entity2.Entity.Fields[0].Name);
		}

		[Fact]
		public void SetNotExistColumnToPrimary()
		{
			var exception = Assert.Throws<SpiderException>(() =>
			{
				var entityMetadata = EntitySpider.PaserEntityMetaData(typeof(Entity1).GetTypeInfo());
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
				var entityMetadata = EntitySpider.PaserEntityMetaData(typeof(Entity2).GetTypeInfo());
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
				var entityMetadata = EntitySpider.PaserEntityMetaData(typeof(Entity3).GetTypeInfo());
				TestPipeline pipeline = new TestPipeline();
				pipeline.InitiEntity(entityMetadata);
			});
			Assert.Equal("Columns set as unique is not a property of your entity.", exception.Message);
		}
	}
}
