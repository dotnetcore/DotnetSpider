using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using Xunit;
namespace DotnetSpider.Test.Selector
{
	
	public class SelectorTest
	{
		private string _html = "<div><a href='http://whatever.com/aaa'></a></div><div><a href='http://whatever.com/bbb'></a></div>";

		[Fact]
		public void TestChain()
		{
			Selectable selectable = new Selectable(_html, "", ContentType.Html);
			var linksWithoutChain = selectable.Links().GetValues();
			ISelectable xpath = selectable.XPath("//div");
			var linksWithChainFirstCall = xpath.Links().GetValues();
			var linksWithChainSecondCall = xpath.Links().GetValues();
			Assert.Equal(linksWithoutChain.Count, linksWithChainFirstCall.Count);
			Assert.Equal(linksWithChainFirstCall.Count, linksWithChainSecondCall.Count);
		}

		[Fact]
		public void TestNodes()
		{
			Selectable selectable = new Selectable(_html, "", ContentType.Html);
			var links = selectable.XPath(".//a/@href").Nodes();
			Assert.Equal(links[0].GetValue(), "http://whatever.com/aaa");

			var links1 = selectable.XPath(".//a/@href").GetValue();
			Assert.Equal(links1, "http://whatever.com/aaa");
		}
	}
}
