using DotnetSpider.Core.Infrastructure;
using Xunit;

namespace DotnetSpider.Core.Test.Infrastructure
{

	public class HttpSenderTest
	{
		[Fact]
		public void Get()
		{
			var result = HttpSender.Request(new HttpRequest
			{
				Url = "https://www.cnblogs.com/"
			});
			Assert.Contains("博客园", result.Html);
		}
	}
}
