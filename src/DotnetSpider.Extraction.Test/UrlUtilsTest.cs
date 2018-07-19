using Xunit;

namespace DotnetSpider.Extraction.Test
{
	public class UrlUtilsTest
	{
		[Fact(DisplayName = "FixRelativeUrl")]
		public void TestFixRelativeUrl()
		{
			string absoluteUrl = Selectable.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/com?aa", absoluteUrl);

			absoluteUrl = Selectable.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl = Selectable.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/..aa", absoluteUrl);

			absoluteUrl = Selectable.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl = Selectable.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/aa", absoluteUrl);
		}
	}
}
