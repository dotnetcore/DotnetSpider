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
				Url = "http://163.com"
			});
			Assert.IsTrue(result.Html.Contains("网易"));
		}
	}
}
