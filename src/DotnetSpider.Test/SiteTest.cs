using DotnetSpider.Core;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace DotnetSpider.Test
{
	[TestClass]
	public class SiteTest
	{

		private const string WName = "WebSite";
		private const string WValue = "12580emall";
		private const string WDomain = "www.12580emall.com"; // 沪动商城
		private const string Url = @"http://www.12580emall.com/emall/mall/index.html";
		private readonly Site _site = new Site() { Domain = WDomain, EncodingName = "UTF-8", Timeout = 3000, };

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
			_site.ClearStartRequests();
			_site.AddStartUrl(Url);
			_site.AddStartRequest(new Request(Url, 1, null));
			Assert.AreEqual(_site.Domain, WDomain);
			Assert.IsTrue(_site.StartRequests.Contains(new Request(Url, 1, null)));
		}

		[TestMethod]
		public void TestAddHeaders()
		{
			_site.AddHeader(WName, WValue);
			Assert.IsNotNull(_site.Headers);
			Assert.IsTrue(_site.Headers.Count > 0);
		}
	}
}
