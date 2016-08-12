using DotnetSpider.Core;
using Xunit;

namespace DotnetSpider.Test
{
	
	public class SiteTest
	{

		private const string WName = "WebSite";
		private const string WValue = "12580emall";
		private const string WDomain = "www.12580emall.com"; // 沪动商城
		private const string Url = @"http://www.12580emall.com/emall/mall/index.html";
		private readonly Site _site = new Site() { Domain = WDomain, EncodingName = "UTF-8", Timeout = 3000, };

		[Fact]
		public void TestAddCookies()
		{
			//Site.AddCookie(wName, wValue);
			//Site.AddCookie(wDomain, wName, wValue);
			//Assert.IsNotNull(Site.AllCookies[wDomain]);
			//Assert.IsNotNull(Site.AllCookies[wDomain][wName]);
			//Assert.Equal(wValue, Site.AllCookies[wDomain][wName]);
		}

		[Fact]
		public void TestAddRequests()
		{
			_site.ClearStartRequests();
			_site.AddStartUrl(Url);
			_site.AddStartRequest(new Request(Url, 1, null));
			Assert.Equal(_site.Domain, WDomain);
			Assert.True(_site.StartRequests.Contains(new Request(Url, 1, null)));
		}

		[Fact]
		public void TestAddHeaders()
		{
			_site.AddHeader(WName, WValue);
			Assert.NotNull(_site.Headers);
			Assert.True(_site.Headers.Count > 0);
		}
	}
}
