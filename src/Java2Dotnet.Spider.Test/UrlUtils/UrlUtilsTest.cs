#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.UrlUtils
{
	[TestClass]
	public class UrlUtilsTest
	{

		[TestMethod]
		public void TestFixRelativeUrl()
		{
			string absoluteUrl = Core.Utils.UrlUtils.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/ss/com?aa");

			absoluteUrl = Core.Utils.UrlUtils.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Utils.UrlUtils.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/ss/..aa");

			absoluteUrl = Core.Utils.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Utils.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/aa");
		}

		[TestMethod]
		public void TestFixAllRelativeHrefs()
		{
			//String originHtml = "<a href=\"/start\">";
			//String replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.AreEqual(replacedHtml, "<a href=\"http://www.dianping.com/start\">");

			//originHtml = "<a href=\"/start a\">";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.AreEqual(replacedHtml, "<a href=\"http://www.dianping.com/start%20a\">");

			//originHtml = "<a href='/start a'>";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.AreEqual(replacedHtml, "<a href=\"http://www.dianping.com/start%20a\">");

			//originHtml = "<a href=/start tag>";
			//replacedHtml = UrlUtils.FixAllRelativeHrefs(originHtml, "http://www.dianping.com/");
			//Assert.AreEqual(replacedHtml, "<a href=\"http://www.dianping.com/start\" tag>");
		}

		[TestMethod]
		public void TestGetDomain()
		{
			string url = "http://www.dianping.com/aa/";
			Assert.AreEqual("www.dianping.com", Core.Utils.UrlUtils.GetDomain(url));

			url = "www.dianping.com/aa/";
			Assert.AreEqual("www.dianping.com", Core.Utils.UrlUtils.GetDomain(url));

			url = "http://www.dianping.com";
			Assert.AreEqual("www.dianping.com", Core.Utils.UrlUtils.GetDomain(url));
		}
	}
}
