using System.Linq;
using Xunit;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Test
{
	public class SelectorTest
	{
		string _html3 =
			"<div><h1>test<a href=\"http://a.com\">aabbcc</a><a href=\"http://a.com/bbc\">aabbcc</a><a href=\"http://b.com\">aabbcc</a></h1></div>"
		;

		[Fact(DisplayName = "Selector_RemoveOutboundLinks")]
		public void RemoveOutboundLinks()
		{
			var request = new Request("http://a.com");
			Page page = new Page(request)
			{
				Content = _html3
			};
			var results = page.Selectable(true).Links().GetValues();
			Assert.Equal(2, results.Count());
			Assert.Equal("http://a.com", results.ElementAt(0));
			Assert.Equal("http://a.com/bbc", results.ElementAt(1));
		}
	}
}
