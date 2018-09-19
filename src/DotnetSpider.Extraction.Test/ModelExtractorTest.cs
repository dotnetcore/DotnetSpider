
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DotnetSpider.Extension.Test.Model
{
	public class ModelExtractorTest
	{
		private string Html = @"
<div class='title'>i am title</div>
<div class='dotnetspider'>i am dotnetspider</div>
<div>
	<div class='aaaa'>a</div>
	<div class='aaaa'>b</div>
</div>
";

		[Fact(DisplayName = "NullModelSelector")]
		public void NullModelSelector()
		{
			var fields = new[]
			{
				new Field( "./div[@class='title']", "title"),
				new Field( "./div[@class='dotnetspider']", "dotnetspider"),
			};
			var model = new ModelDefinition(null, fields);
			var extractor = new ModelExtractor();

			var result = extractor.Extract(CreatePage(), model).First() as Dictionary<string, dynamic>;

			Assert.Equal("i am title", result["title"]);
			Assert.Equal("i am dotnetspider", result["dotnetspider"]);
		}

		[Fact(DisplayName = "ModelSelector")]
		public void ModelSelector()
		{
			var entitySelector = new Selector("//div[@class='aaaa']");
			var fields = new[]
			{
				new Field( ".", "title"),
			};
			var model = new ModelDefinition(entitySelector, fields);
			ModelExtractor extractor = new ModelExtractor();

			var results = extractor.Extract(CreatePage(), model).ToList();
			Assert.Equal(2, results.Count());

			Assert.Equal("a", results[0]["title"]);
			Assert.Equal("b", results[1]["title"]);
		}

		[Fact(DisplayName = "NullEntityModelSelector")]
		public void NullEntityModelSelector()
		{
			ModelExtractor extractor = new ModelExtractor();
			var model = new ModelDefinition<N>();

			var result = extractor.Extract(CreatePage(), model).First() as Dictionary<string, dynamic>;

			Assert.Equal("i am title", result["title"]);
			Assert.Equal("i am dotnetspider", result["dotnetspider"]);
		}

		[Fact(DisplayName = "EntityModelSelector")]
		public void EntityModelSelector()
		{
			ModelExtractor extractor = new ModelExtractor();
			var model = new ModelDefinition<E>();

			var results = extractor.Extract(CreatePage(), model).ToList();
			Assert.Equal(2, results.Count());

			Assert.Equal("a", results[0]["title"]);
			Assert.Equal("b", results[1]["title"]);
		}

		private class N : IBaseEntity
		{
			[Field(Expression = "./div[@class='title']")]
			public string title { get; set; }

			[Field(Expression = "./div[@class='dotnetspider']")]
			public string dotnetspider { get; set; }
		}

		[Entity(Expression = "//div[@class='aaaa']")]
		private class E : IBaseEntity
		{
			[Field(Expression = ".")]
			public string title { get; set; }
		}

		private Selectable CreatePage()
		{
			return new Selectable(Html);
		}
	}
}
