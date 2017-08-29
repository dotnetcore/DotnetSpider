using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Processor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using DotnetSpider.Core;
using System.IO;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class DataHandlerTest
	{
		class MyDataHanlder : DataHandler
		{
			public string Identity { get; set; }

			public MyDataHanlder(string guid)
			{
				Identity = guid;
			}

			protected override DataObject HandleDataOject(DataObject data, Page page)
			{
				return data;
			}

			public override List<DataObject> Handle(List<DataObject> datas, Page page)
			{
				var stream = File.Create(Identity);
				stream.Dispose();
				return base.Handle(datas, page);
			}
		}

		[TestMethod]
		public void HandlerWhenExtractZeroResult()
		{
			var entityMetadata = EntitySpider.GenerateEntityDefine(typeof(Product).GetTypeInfo());
			var identity = Guid.NewGuid().ToString("N");
			entityMetadata.DataHandler = new MyDataHanlder(identity);
			EntityProcessor processor = new EntityProcessor(new Site(), entityMetadata);
			processor.Process(new Page(new Request("http://www.abcd.com"))
			{
				Content = "{'data':[{'name':'1'},{'name':'2'}]}"
			});
			Assert.IsTrue(File.Exists(identity));
			File.Delete(identity);
		}

		[EntitySelector(Expression = "$.data[*]", Type = Core.Selector.SelectorType.JsonPath)]
		public class Product : SpiderEntity
		{
			[PropertyDefine(Expression = "$.name", Type = Core.Selector.SelectorType.JsonPath, Length = 100)]
			public string name { get; set; }
		}
	}
}
