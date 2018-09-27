using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using Xunit;

namespace DotnetSpider.Extension.Test
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

			processor.Process(new Page(new Request("http://www.abcd.com"))
			{
				Content = "{'data':[{'name':'aaaa'},{'name':'bbbb'}]}"
			});
			Assert.True(File.Exists("file.aaaa"));
			Assert.True(File.Exists("file.bbbb"));
			File.Delete("file.aaaa");
			File.Delete("file.bbbb");
		}

		[Entity(Expression = "$.data[*]", Type = SelectorType.JsonPath)]
		[Schema]
		private class Product : IBaseEntity
		{
			[Field(Expression = "$.name", Type = SelectorType.JsonPath)]
			[Column(Length = 100)]
			public string name { get; set; }
		}
	}
}
