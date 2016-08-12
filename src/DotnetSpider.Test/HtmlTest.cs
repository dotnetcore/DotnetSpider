using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using Xunit;
namespace DotnetSpider.Test
{
	
	public class HtmlTest
	{
		[Fact]
		public void TestRegexSelector()
		{
			Selectable selectable = new Selectable("aaaaaaab", "", ContentType.Html);
			//        Assert.assertEquals("abbabbab", (selectable.regex("(.*)").replace("aa(a)", "$1bb").toString()));
			string value = selectable.Regex("(.*)").GetValue();
			Assert.Equal("aaaaaaab", value);

		}
	}
}
