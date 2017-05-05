using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;


namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class PageTest
	{
		public static Request GetRequest()
		{
			var extras = new Dictionary<string, dynamic> { { "Test", "Forever" } };
			var request = new Request("http://www.taobao.com", extras)
			{
				Method = "get",
				Priority = 1
			};
			return request;
		}

		[TestMethod]
		public void Deep()
		{
			var request = GetRequest();
			Page page = new Page(request, ContentType.Html);
			page.AddTargetRequest("http://taobao.com/bbb");
			Assert.AreEqual(page.TargetRequests.First().Depth, 2);
		}
	}
}
