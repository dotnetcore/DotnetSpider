using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using Xunit;
using System;
using DotnetSpider.Core;
using System.IO;

namespace DotnetSpider.Extension.Test
{

	public class DataHandlerTest
	{
		private class MyDataHanlder : IDataHandler
		{
			public string Identity { get; set; }

			public MyDataHanlder(string guid)
			{
				Identity = guid;
			}

			public void Handle(ref dynamic data, Page page)
			{
				var stream = File.Create(Identity);
				stream.Dispose();
			}
		}

		[Fact]
		public void HandlerWhenExtractZeroResult()
		{
			var entityMetadata = new ModelDefine<Product>();
			var identity = Guid.NewGuid().ToString("N");

			EntityProcessor<Product> processor = new EntityProcessor<Product>(null, null, new MyDataHanlder(identity));

			processor.Process(new Page(new Request("http://www.abcd.com") { Site = new Site() })
			{
				Content = "{'data':[{'name':'1'},{'name':'2'}]}"
			}, new DefaultSpider());
			Assert.True(File.Exists(identity));
			File.Delete(identity);
		}

		[EntitySelector(Expression = "$.data[*]", Type = Core.Selector.SelectorType.JsonPath)]
		private class Product
		{
			[Field(Expression = "$.name", Type = Core.Selector.SelectorType.JsonPath, Length = 100)]
			public string name { get; set; }
		}
	}
}
