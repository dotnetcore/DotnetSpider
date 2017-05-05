using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class UrlUtilsTest
	{
		[TestMethod]
		public void TestFixRelativeUrl()
		{
			string absoluteUrl = Core.Infrastructure.UrlUtils.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/ss/com?aa");

			absoluteUrl = Core.Infrastructure.UrlUtils.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Infrastructure.UrlUtils.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/ss/..aa");

			absoluteUrl = Core.Infrastructure.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/sh/aa");

			absoluteUrl = Core.Infrastructure.UrlUtils.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.AreEqual(absoluteUrl, "http://www.dianping.com/aa");
		}

		[TestMethod]
		public void TestGetDomain()
		{
			string url = "http://www.dianping.com/aa/";
			Assert.AreEqual("www.dianping.com", Core.Infrastructure.UrlUtils.GetDomain(url));

			url = "www.dianping.com/aa/";
			Assert.AreEqual("www.dianping.com", Core.Infrastructure.UrlUtils.GetDomain(url));

			url = "http://www.dianping.com";
			Assert.AreEqual("www.dianping.com", Core.Infrastructure.UrlUtils.GetDomain(url));
		}
	}
}
