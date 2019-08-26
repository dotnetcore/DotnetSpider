using System.Collections.Generic;
using DotnetSpider.Downloader;
using Xunit;

namespace DotnetSpider.Tests.Downloader
{
	public class RequestTests
	{
		[Fact(DisplayName = "PubAndSub")]
		public void ComputeHash()
		{
			var request1 = new Request("http://www.baidu.com", new Dictionary<string, string> {{"a", "b"}})
			{
				UserAgent = "dotnetspider"
			};
			request1.ComputeHash();
			var request2 = new Request("http://www.baidu.com", new Dictionary<string, string> {{"a", "b"}})
			{
				UserAgent = "dotnetspider"
			};
			request2.ComputeHash();
			Assert.Equal(request1.Hash, request2.Hash);
		}
	}
}
