using Java2Dotnet.Spider.Core.Selector;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Selector
{
	[TestClass]
	public class ExtractorsTest
	{
		string _html = "<div><h1>test<a href=\"xxx\">aabbcc</a></h1></div>";
		string _html2 = "<title>aabbcc</title>";

		[TestMethod]
		public void TestEach()
		{
			Assert.AreEqual(Selectors.Css("div h1 a").Select(_html).OuterHtml, "<a href=\"xxx\">aabbcc</a>");
			Assert.AreEqual(Selectors.Css("div h1 a", "href").Select(_html), "xxx");
			Assert.AreEqual(Selectors.Css("div h1 a").Select(_html).InnerHtml, "aabbcc");
			Assert.AreEqual(Selectors.XPath("//a/@href").Select(_html), "xxx");
			Assert.AreEqual(Selectors.Regex("a href=\"(.*)\"").Select(_html), "a href=\"xxx\"");
			Assert.AreEqual(Selectors.Regex("(a href)=\"(.*)\"", 2).Select(_html), "xxx");
		}
	}
}
