using DotnetSpider.Common;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
				Method = HttpMethod.Get,
				Priority = 1
			};
			return request;
		}

		[Fact(DisplayName = "Page_Deep")]
		public void Deep()
		{
			var request = GetRequest();
			Page page = new Page(request);
			page.AddTargetRequest("http://taobao.com/bbb");
			Assert.Equal(2, page.TargetRequests.First().Depth);
		}
	}
}
