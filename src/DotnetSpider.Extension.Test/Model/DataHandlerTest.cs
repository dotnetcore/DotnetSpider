using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using Xunit;
using System;
using DotnetSpider.Core;
using System.IO;

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
			var model = new ModelDefine<Product>();
			EntityProcessor<Product> processor = new EntityProcessor<Product>(null, null, new MyDataHandler());

			processor.Process(new Page(new Request("http://www.abcd.com") { Site = new Site() })
			{
				Content = "{'data':[{'name':'aaaa'},{'name':'bbbb'}]}"
			}, new DefaultSpider());
			Assert.True(File.Exists("file.aaaa"));
			Assert.True(File.Exists("file.bbbb"));
			File.Delete("file.aaaa");
			File.Delete("file.bbbb");
		}

		[EntitySelector(Expression = "$.data[*]", Type = Core.Selector.SelectorType.JsonPath)]
		private class Product
		{
			[Field(Expression = "$.name", Type = Core.Selector.SelectorType.JsonPath, Length = 100)]
			public string name { get; set; }
		}
	}
}
