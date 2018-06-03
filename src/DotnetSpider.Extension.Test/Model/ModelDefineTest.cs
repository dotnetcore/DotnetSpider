using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DotnetSpider.Extension.Test.Model
{
	public class ModelDefineTest
	{
		[Fact]
		public void NullModelSelector()
		{
			var fields = new[]
			{
				new Field( "./div[1]/a/@href", "Url"),
				new Field( "./@data-sku", "Sku"),
			};
			ModelDefine model = new ModelDefine(null, fields);
			Assert.Null(model.Selector);
			Assert.Equal(2, model.Fields.Count);
			Assert.True(Guid.TryParse(model.Identity, out _));
		}

		/// <summary>
		/// Tmp data
		/// </summary>
		[Fact]
		public void NullTableInfo()
		{
			var entitySelector = new Selector("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]");
			var fields = new[]
			{
				new Field( "./div[1]/a/@href", "Url"),
				new Field( "./@data-sku", "Sku"),
			};
			ModelDefine model = new ModelDefine(entitySelector, fields);
			Assert.Equal("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", model.Selector.Expression);
			Assert.Equal(SelectorType.XPath, model.Selector.Type);
			Assert.Equal(2, model.Fields.Count);
			Assert.True(Guid.TryParse(model.Identity, out _));
		}

		[Fact]
		public void TableInfo()
		{
			var entitySelector = new Selector("//li[@class='gl-item']/div[contains(@class,'j-sku-item')]");
			var fields = new[]
			{
				new Field( "./div[1]/a/@href", "Url"),
				new Field( "./@data-sku", "Sku"),
			};
			var tableInfo = new TableInfo("db01", "tb1");
			ModelDefine model = new ModelDefine(entitySelector, fields, tableInfo);
			Assert.Equal(2, model.Fields.Count);
			Assert.Equal("db01.tb1", model.Identity);
		}

		[Fact]
		public void NullTableInfoEntityModelDefine()
		{
			ModelDefine<NullTableInfoEntity> model = new ModelDefine<NullTableInfoEntity>();
			Assert.Equal(2, model.Fields.Count);

			var field1 = model.Fields.First();
			Assert.Equal("CategoryName", field1.Name);
			Assert.Equal("cat", field1.Expression);
			Assert.Equal(SelectorType.Enviroment, field1.Type);
			Assert.Equal(DataType.String, field1.DataType);

			var field2 = model.Fields.ElementAt(1);
			Assert.Equal(10, field2.Length);

			Assert.Null(model.TableInfo);
			Assert.Equal("DotnetSpider.Extension.Test.Model.ModelDefineTest+NullTableInfoEntity", model.Identity);
		}

		[Fact]
		public void TableInfoEntityModelDefine()
		{
			ModelDefine<TableInfoEntity> model = new ModelDefine<TableInfoEntity>();
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
			[Field(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[Field(Expression = "./@jdzy_shop_id", Length = 10)]
			public string JdzyShopId { get; set; }
		}

		[TableInfo("db01", "tb1")]
		[EntitySelector(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]")]
		private class TableInfoEntity
		{
			[Field(Expression = "cat", Type = SelectorType.Enviroment)]
			public string CategoryName { get; set; }

			[Field(Expression = "./@jdzy_shop_id", Length = 10)]
			public string JdzyShopId { get; set; }
		}
	}
}
