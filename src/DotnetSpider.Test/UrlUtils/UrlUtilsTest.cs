using Xunit;

namespace DotnetSpider.Test.UrlUtils
{
	
	public class UrlUtilsTest
	{

		[Fact]
		public void TestFixRelativeUrl()
		{
			string absoluteUrl = Core.Common.UrlUtils.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal(absoluteUrl, "http://www.dianping.com/sh/ss/com?aa");

			absoluteUrl = Core.Common.UrlUtils.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Common.UrlUtils.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal(absoluteUrl, "http://www.dianping.com/sh/ss/..aa");

			absoluteUrl = Core.Common.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.Equal(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Common.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal(absoluteUrl, "http://www.dianping.com/aa");
		}

		[Fact]
		public void TestFixAllRelativeHrefs()
		{
			//String originHtml = "<a href=\"/start\">";
			//String replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.Equal(replacedHtml, "<a href=\"http://www.dianping.com/start\">");

			//originHtml = "<a href=\"/start a\">";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.Equal(replacedHtml, "<a href=\"http://www.dianping.com/start%20a\">");

			//originHtml = "<a href='/start a'>";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.Equal(replacedHtml, "<a href=\"http://www.dianping.com/start%20a\">");

			//originHtml = "<a href=/start tag>";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.Equal(replacedHtml, "<a href=\"http://www.dianping.com/start\" tag>");
		}

		[Fact]
		public void TestGetDomain()
		{
			string url = "http://www.dianping.com/aa/";
			Assert.Equal("www.dianping.com", Core.Common.UrlUtils.GetDomain(url));

			url = "www.dianping.com/aa/";
			Assert.Equal("www.dianping.com", Core.Common.UrlUtils.GetDomain(url));

			url = "http://www.dianping.com";
			Assert.Equal("www.dianping.com", Core.Common.UrlUtils.GetDomain(url));
		}
	}
}
