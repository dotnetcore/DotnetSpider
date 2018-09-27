using System;
using System.Linq;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using Xunit;

namespace DotnetSpider.Extraction.Test
{
	public class ModelDefineTest
	{

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
			Assert.Equal(typeof(Entity9).FullName, entity3.Identity);
		}

		[Fact(DisplayName = "NullModelSelector")]
		public void NullModelSelector()
		{
			var fields = new[]
			{
				new Field( "./div[1]/a/@href", "Url"),
				new Field( "./@data-sku", "Sku"),
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
				new Field( "./div[1]/a/@href", "Url"),
				new Field( "./@data-sku", "Sku"),
			};
			ModelDefinition model = new ModelDefinition(entitySelector, fields);
			Assert.Equal("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", model.Selector.Expression);
			Assert.Equal(SelectorType.XPath, model.Selector.Type);
			Assert.Equal(2, model.Fields.Count);
			Assert.True(Guid.TryParse(model.Identity, out _));
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

			var field2 = model.Fields.ElementAt(1);
			Assert.Equal(typeof(TableInfoEntity).FullName, model.Identity);
		}

		[Entity(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class NullTableInfoEntity : IBaseEntity
		{
			[Field(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[Field(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }
		}

		[Entity(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class TableInfoEntity : IBaseEntity
		{
			[Field(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[Field(Expression = "./@jdzy_shop_id")]
			public string JdzyShopId { get; set; }
		}

		[Entity(Expression = "expression")]
		private class Entity7 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		[Entity(Expression = "expression2", Type = SelectorType.Css)]
		private class Entity8 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		private class Entity9 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		private class Entity4 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Name { get; set; }
		}

		private class Entity14 : IBaseEntity
		{
			[Field(Expression = "Url")]
			public string Url { get; set; }
		}

		private class Entity10 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Name { get; set; }

			[Field(Expression = "")]
			public string Name2 { get; set; }

			[Field(Expression = "")]
			public string Name3 { get; set; }
		}

		private class Entity18 : IBaseEntity
		{
			[Field(Expression = "")]
			public string c1 { get; set; }
		}

		private class Entity19 : IBaseEntity
		{
			[Field(Expression = "")]
			public string c1 { get; set; }
		}

		private class Entity11 : IBaseEntity
		{
			[ReplaceFormatter(NewValue = "a", OldValue = "b")]
			[RegexFormatter(Pattern = "a(*)")]
			[Field(Expression = "Name")]
			public string Name { get; set; }
		}


		private class Entity3 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Url { get; set; }
		}

		private class Entity2 : IBaseEntity
		{
			[Field(Expression = "")]
			public string Url { get; set; }
		}
	}
}
