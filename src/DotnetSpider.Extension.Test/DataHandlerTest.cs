using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Processor;
using Xunit;
using DotnetSpider.Core;
using System.IO;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Common;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction;

namespace DotnetSpider.Extension.Test.Model
{
	public class DataHandlerTest
	{
		private class MyDataHandler : IDataHandler
		{
			public void Handle(ref dynamic data, Page page)
			{
				var name = data.name;
				var stream = File.Create("file." + name);
				stream.Dispose();
			}
		}

		[Fact(DisplayName = "HandleModel")]
		public void HandleModel()
		{
			var model = new ModelDefinition<Product>();
			EntityProcessor<Product> processor = new EntityProcessor<Product>(null, null, new MyDataHandler());

			processor.Process(new Page(new Request("http://www.abcd.com") { Site = new Site() })
			{
				Content = "{'data':[{'name':'aaaa'},{'name':'bbbb'}]}"
			}, null);
			Assert.True(File.Exists("file.aaaa"));
			Assert.True(File.Exists("file.bbbb"));
			File.Delete("file.aaaa");
			File.Delete("file.bbbb");
		}

		[EntitySelector(Expression = "$.data[*]", Type = SelectorType.JsonPath)]
		private class Product
		{
			[FieldSelector(Expression = "$.name", Type = SelectorType.JsonPath, Length = 100)]
			public string name { get; set; }
		}
	}
}
