using System.Linq;
using Xunit;

namespace DotnetSpider.Extraction.Test
{
	public class HtmlTest
	{
		[Fact(DisplayName = "HtmlSelect")]
		public void Select()
		{
			Selectable selectable = new Selectable("aaaaaaab");
			string value = selectable.Regex("(.*)").GetValue();
			Assert.Equal("aaaaaaab", value);
		}

		[Fact(DisplayName = "DonotFixAllRelativeHrefs")]
		public void DonotFixAllRelativeHrefs()
		{
			Selectable selectable = new Selectable("<div><a href=\"aaaa.com\">aaaaaaab</a></div>", null);
			var values = selectable.XPath(".//a").GetValues();
			Assert.Equal("aaaaaaab", values.First());
		}

		[Fact(DisplayName = "FixAllRelativeHrefs")]
		public void FixAllRelativeHrefs()
		{
			// 只有相对路径需要做补充
			Selectable selectable1 = new Selectable("<div><a href=\"/a/b\">aaaaaaab</a></div>", "http://www.b.com");
			var value1 = selectable1.XPath(".//a").Links().GetValue();
			Assert.Equal("http://www.b.com/a/b", value1);
			
			// 绝对路径不需要做补充
			Selectable selectable2 = new Selectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
				"http://www.b.com", false);
			var value2 = selectable2.XPath(".//a").GetValues().First();
			Assert.Equal("aaaaaaab", value2);
		}

		[Fact(DisplayName = "RemoveOutboundLinks")]
		public void RemoveOutboundLinks()
		{
			// 绝对路径不需要做补充
			Selectable selectable2 = new Selectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
				"http://www.b.com", true);
			var value2 = selectable2.XPath(".//a").GetValues();
			Assert.Empty( value2);
		}
	}
}