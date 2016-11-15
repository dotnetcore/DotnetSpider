using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Core.Test
{
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

		[Fact]
		public void Deep()
		{
			var request = GetRequest();
			Page page = new Page(request, ContentType.Html);
			page.AddTargetRequest("http://taobao.com/bbb");
			Assert.Equal(page.TargetRequests.First().Depth, 2);
		}
	}
}
