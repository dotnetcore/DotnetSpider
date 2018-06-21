using DotnetSpider.Core.Infrastructure;
using Xunit;

namespace DotnetSpider.Core.Test.Infrastructure
{

	public class HttpSenderTest
	{
		[Fact(DisplayName = "HttpSender_Get")]
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
