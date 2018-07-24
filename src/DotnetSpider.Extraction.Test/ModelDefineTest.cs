using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System;
using System.Linq;
using Xunit;

namespace DotnetSpider.Extension.Test.Model
{
	public class ModelDefineTest
	{
		[Fact(DisplayName = "NotExistColumnAsIndex")]
		public void NotExistColumnAsIndex()
		{
			try
			{
				var entityMetadata = new ModelDefinition<Entity2>();
				throw new Exception("Test failed");
			}
			catch (ArgumentException exception)
			{
				Assert.Equal("Columns set as index are not a property of your entity", exception.Message);
			}
		}

		[Fact(DisplayName = "NotExistColumnAsUnique")]
		public void NotExistColumnAsUnique()
		{
			try
			{
				var entityMetadata = new ModelDefinition<Entity3>();

				throw new Exception("Test failed");
			}
			catch (ArgumentException exception)
			{
				Assert.Equal("Columns set as unique are not a property of your entity", exception.Message);
			}
		}

		[Fact(DisplayName = "Formaters")]
		public void Formaters()
		{
			var entity1 = new ModelDefinition<Entity11>();
			var fields = entity1.Fields.ToArray();
			var formatters = (fields[0]).Formatters;
			Assert.Equal(2, formatters.Length);
			var replaceFormatter = (ReplaceFormatter)formatters[0];
			Assert.Equal("a", replaceFormatter.NewValue);
			Assert.Equal("b", replaceFormatter.OldValue);
		}

		[Fact(DisplayName = "ColumnOfIndexesOverLength")]
		public void ColumnOfIndexesOverLength()
		{
			try
			{
				var entity = new ModelDefinition<Entity19>();

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of index should not large than 256", e.Message);
			}
		}

		[Fact(DisplayName = "ColumnOfUniqueOverLength")]
		public void ColumnOfUniqueOverLength()
		{
			try
			{
				var entity = new ModelDefinition<Entity18>();

				throw new Exception("Failed.");
			}
			catch (Exception e)
			{
				Assert.Equal("Column length of unique should not large than 256", e.Message);
			}
		}

		[Fact(DisplayName = "Indexes")]
		public void Indexes()
		{
			var entity1 = new ModelDefinition<Entity10>();
			Assert.Equal("Name3", entity1.Table.Indexs[0]);
			Assert.Equal(2, entity1.Table.Uniques.Length);
			Assert.Equal("Name,Name2", entity1.Table.Uniques[0]);
			Assert.Equal("Name2", entity1.Table.Uniques[1]);
		}

		[Fact(DisplayName = "Schema")]
		public void Schema()
		{
			var entityMetadata = new ModelDefinition<Entity4>();

			Assert.Equal("test", entityMetadata.Table.Database);
			Assert.Equal(TableNamePostfix.Monday, entityMetadata.Table.Postfix);

			var entityMetadata1 = new ModelDefinition<Entity14>();
			Assert.Null(entityMetadata1.Table);
		}

		[Fact(DisplayName = "EntitySelector")]
		public void EntitySelector()
		{
			var entity1 = new ModelDefinition<Entity7>();
			Assert.Equal("expression", entity1.Selector.Expression);
			Assert.Equal(SelectorType.XPath, entity1.Selector.Type);


			var entity2 = new ModelDefinition<Entity8>();
			Assert.Equal("expression2", entity2.Selector.Expression);
			Assert.Equal(SelectorType.Css, entity2.Selector.Type);

			var entity3 = new ModelDefinition<Entity9>();
			Assert.Null(entity3.Selector);
			Assert.Equal("test.table", entity3.Identity);
		}

		[Fact(DisplayName = "NullModelSelector")]
		public void NullModelSelector()
		{
			var fields = new[]
			{
				new FieldSelector( "./div[1]/a/@href", "Url"),
				new FieldSelector( "./@data-sku", "Sku"),
			};
			ModelDefinition model = new ModelDefinition(null, fields);
			Assert.Null(model.Selector);
			Assert.Equal(2, model.Fields.Count);
			Assert.True(Guid.TryParse(model.Identity, out _));
		}

		/// <summary>
		/// Tmp data
		/// </summary>
		[Fact(DisplayName = "NullTableInfo")]
		public void NullTableInfo()
		{
			var entitySelector = new Selector("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]");
			var fields = new[]
			{
				new FieldSelector( "./div[1]/a/@href", "Url"),
				new FieldSelector( "./@data-sku", "Sku"),
			};
			ModelDefinition model = new ModelDefinition(entitySelector, fields);
			Assert.Equal("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", model.Selector.Expression);
			Assert.Equal(SelectorType.XPath, model.Selector.Type);
			Assert.Equal(2, model.Fields.Count);
			Assert.True(Guid.TryParse(model.Identity, out _));
		}

		[Fact(DisplayName = "TableInfo")]
		public void TableInfo()
		{
			var entitySelector = new Selector("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]");
			var fields = new[]
			{
				new FieldSelector( "./div[1]/a/@href", "Url"),
				new FieldSelector( "./@data-sku", "Sku"),
			};
			var tableInfo = new TableInfo("db01", "tb1");
			ModelDefinition model = new ModelDefinition(entitySelector, fields, tableInfo);
			Assert.Equal(2, model.Fields.Count);
			Assert.Equal("db01.tb1", model.Identity);
		}

		[Fact(DisplayName = "NullTableInfoEntityModelDefine")]
		public void NullTableInfoEntityModelDefine()
		{
			ModelDefinition<NullTableInfoEntity> model = new ModelDefinition<NullTableInfoEntity>();
			Assert.Equal(2, model.Fields.Count);

			var field1 = model.Fields.First();
			Assert.Equal("CategoryName", field1.Name);
			Assert.Equal("cat", field1.Expression);
			Assert.Equal(SelectorType.Enviroment, field1.Type);
			Assert.Equal(DataType.String, field1.DataType);

			var field2 = model.Fields.ElementAt(1);
			Assert.Equal(10, field2.Length);

			Assert.Null(model.Table);
			Assert.Equal("DotnetSpider.Extension.Test.Model.ModelDefineTest+NullTableInfoEntity", model.Identity);
		}

		[Fact(DisplayName = "TableInfoEntityModelDefine")]
		public void TableInfoEntityModelDefine()
		{
			ModelDefinition<TableInfoEntity> model = new ModelDefinition<TableInfoEntity>();
			Assert.Equal(2, model.Fields.Count);

			var field1 = model.Fields.First();
			Assert.Equal("CategoryName", field1.Name);
			Assert.Equal("cat", field1.Expression);
			Assert.Equal(SelectorType.Enviroment, field1.Type);
			Assert.Equal(DataType.String, field1.DataType);

			var field2 = model.Fields.ElementAt(1);
			Assert.Equal(10, field2.Length);

			Assert.Equal("db01.tb1", model.Identity);
		}

		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class NullTableInfoEntity
		{
			[FieldSelector(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[FieldSelector(Expression = "./@jdzy_shop_id", Length = 10)]
			public string JdzyShopId { get; set; }
		}

		[TableInfo("db01", "tb1")]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class TableInfoEntity
		{
			[FieldSelector(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[FieldSelector(Expression = "./@jdzy_shop_id", Length = 10)]
			public string JdzyShopId { get; set; }
		}

		[TableInfo("test", "table")]
		[EntitySelector(Expression = "expression")]
		private class Entity7
		{
			[FieldSelector(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		[EntitySelector(Expression = "expression2", Type = SelectorType.Css)]
		private class Entity8
		{
			[FieldSelector(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table")]
		private class Entity9
		{
			[FieldSelector(Expression = "")]
			public string Name { get; set; }
		}

		[TableInfo("test", "table", TableNamePostfix.Monday)]
		private class Entity4
		{
			[FieldSelector(Expression = "")]
			public string Name { get; set; }
		}

		private class Entity14
		{
			[FieldSelector(Expression = "Url")]
			public string Url { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "Name3" }, Uniques = new[] { "Name,Name2", "Name2" })]
		private class Entity10
		{
			[FieldSelector(Expression = "", Length = 100)]
			public string Name { get; set; }

			[FieldSelector(Expression = "", Length = 100)]
			public string Name2 { get; set; }

			[FieldSelector(Expression = "", Length = 100)]
			public string Name3 { get; set; }
		}

		[TableInfo("test", "table", Uniques = new[] { "c1" })]
		private class Entity18
		{
			[FieldSelector(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "c1" })]
		private class Entity19
		{
			[FieldSelector(Expression = "", Length = 300)]
			public string c1 { get; set; }
		}

		[TableInfo("test", "table")]
		private class Entity11
		{
			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			[FieldSelector(Expression = "Name")]
			public string Name { get; set; }
		}


		[TableInfo("test", "table", Uniques = new[] { "c1" })]
		private class Entity3
		{
			[FieldSelector(Expression = "")]
			public string Url { get; set; }
		}

		[TableInfo("test", "table", Indexs = new[] { "c1" })]
		private class Entity2
		{
			[FieldSelector(Expression = "")]
			public string Url { get; set; }
		}
	}
}
