using DotnetSpider.Core.Selector;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class HtmlTest
	{
		[Fact]
		public void Select()
		{
			Selectable selectable = new Selectable("aaaaaaab", "", ContentType.Html);
			string value = selectable.Regex("(.*)").GetValue();
			Assert.Equal("aaaaaaab", value);
		}
	}
}
