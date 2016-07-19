using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Pipeline
{
	[TestClass]
	public class FilePipelineTest
	{
		private ResultItems _resultItems;
		private ISpider _spider;

		[TestInitialize]
		public void Before()
		{
			_resultItems = new ResultItems();
			_resultItems.AddOrUpdateResultItem("content", "爬虫工具");
			Request request = new Request("http://www.baidu.com", 1, null);
			_resultItems.Request = request;
			_spider = new DefaultSpider();

		}
 
		[TestMethod]
		public void TestProcess()
		{
			FilePipeline filePipeline = new FilePipeline();
			filePipeline.Process(_resultItems, _spider);
		}
	}
}
