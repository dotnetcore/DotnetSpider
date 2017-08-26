using DotnetSpider.Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Infrastructure
{
	[TestClass]
	public class HttpSenderTest
	{
		[TestMethod]
		public void Get()
		{
			var result = HttpSender.GetHtml(new HttpRequest
			{
				Url = "https://www.cnblogs.com/"
			});
			Assert.IsTrue(result.Html.Contains("博客园"));
		}
	}
}
