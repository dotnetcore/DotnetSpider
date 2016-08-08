using DotnetSpider.Core;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace DotnetSpider.Test
{
	[TestClass]
	public class SiteTest
	{

		private const string wName = "WebSite";
		private const string wValue = "12580emall";
		private const string wDomain = "www.12580emall.com"; // 沪动商城
		private const string URL = @"http://www.12580emall.com/emall/mall/index.html";
		private Site Site = new Site() { Domain = wDomain, EncodingName = "UTF-8", Timeout = 3000, };

		[TestMethod]
		public void TestAddCookies()
		{
			//Site.AddCookie(wName, wValue);
			//Site.AddCookie(wDomain, wName, wValue);
			//Assert.IsNotNull(Site.AllCookies[wDomain]);
			//Assert.IsNotNull(Site.AllCookies[wDomain][wName]);
			//Assert.AreEqual(wValue, Site.AllCookies[wDomain][wName]);
		}

		[TestMethod]
		public void TestAddRequests()
		{
			Site.ClearStartRequests();
			Site.AddStartUrl(URL);
			Site.AddStartRequest(new Request(URL, 1, null));
			Assert.AreEqual(Site.Domain, wDomain);
			Assert.IsTrue(Site.StartRequests.Contains(new Request(URL, 1, null)));
		}

		[TestMethod]
		public void TestAddHeaders()
		{
			Site.AddHeader(wName, wValue);
			Assert.IsNotNull(Site.Headers);
			Assert.IsTrue(Site.Headers.Count > 0);
		}
	}
}
