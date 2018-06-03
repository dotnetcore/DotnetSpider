using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			ModelDefine model = new ModelDefine(null, fields);
			ModelExtractor extractor = new ModelExtractor();

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
			ModelDefine model = new ModelDefine(entitySelector, fields);
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
			IModel model = new ModelDefine<N>();

			var result = extractor.Extract(CreatePage(), model).First() as Dictionary<string, dynamic>;

			Assert.Equal("i am title", result["title"]);
			Assert.Equal("i am dotnetspider", result["dotnetspider"]);
		}

		[Fact(DisplayName = "EntityModelSelector")]
		public void EntityModelSelector()
		{
			ModelExtractor extractor = new ModelExtractor();
			IModel model = new ModelDefine<E>();

			var results = extractor.Extract(CreatePage(), model).ToList();
			Assert.Equal(2, results.Count());

			Assert.Equal("a", results[0]["title"]);
			Assert.Equal("b", results[1]["title"]);
		}

		private class N
		{
			[Field(Expression = "./div[@class='title']")]
			public string title { get; set; }

			[Field(Expression = "./div[@class='dotnetspider']")]
			public string dotnetspider { get; set; }
		}

		[EntitySelector(Expression = "//div[@class='aaaa']")]
		private class E
		{
			[Field(Expression = ".")]
			public string title { get; set; }
		}

		private Page CreatePage()
		{
			var request = new Request("http://dotnetspoder.me");
			var site = new Site();
			request.Site = site;
			var page = new Page(request);
			page.Content = Html;
			return page;
		}
	}
}
