using Xunit;

namespace DotnetSpider.Core.Test
{

	public class UrlUtilsTest
	{
		[Fact(DisplayName = "UrlUtils_FixRelativeUrl")]
		public void TestFixRelativeUrl()
		{
			string absoluteUrl = Core.Infrastructure.UrlUtil.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/com?aa", absoluteUrl);

			absoluteUrl = Core.Infrastructure.UrlUtil.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl = Core.Infrastructure.UrlUtil.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/..aa", absoluteUrl);

			absoluteUrl = Core.Infrastructure.UrlUtil.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl = Core.Infrastructure.UrlUtil.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/aa", absoluteUrl);
		}

		[Fact(DisplayName = "UrlUtils_GetDomain")]
		public void TestGetDomain()
		{
			string url = "http://www.dianping.com/aa/";
			Assert.Equal("www.dianping.com", Core.Infrastructure.UrlUtil.GetDomain(url));

			url = "www.dianping.com/aa/";
			Assert.Equal("www.dianping.com", Core.Infrastructure.UrlUtil.GetDomain(url));

			url = "http://www.dianping.com";
			Assert.Equal("www.dianping.com", Core.Infrastructure.UrlUtil.GetDomain(url));
		}
	}
}
