using DotnetSpider.Core.Selector;
using Xunit;

namespace DotnetSpider.Test.Selector
{
	
	public class ExtractorsTest
	{
		string _html = "<div><h1>test<a href=\"xxx\">aabbcc</a></h1></div>";
		//string _html2 = "<title>aabbcc</title>";

		[Fact]
		public void TestEach()
		{
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).OuterHtml, "<a href=\"xxx\">aabbcc</a>");
			Assert.Equal(Selectors.Css("div h1 a", "href").Select(_html), "xxx");
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).InnerHtml, "aabbcc");
			Assert.Equal(Selectors.XPath("//a/@href").Select(_html), "xxx");
			Assert.Equal(Selectors.Regex("a href=\"(.*)\"").Select(_html), "a href=\"xxx\"");
			Assert.Equal(Selectors.Regex("(a href)=\"(.*)\"", 2).Select(_html), "xxx");
		}
	}
}
