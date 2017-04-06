using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;


namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class SiteTest
	{
		private string WName = "WebSite";
		private string WValue = "12580emall";
		private string WDomain = "www.12580emall.com";
		private string Url = @"http://www.12580emall.com/emall/mall/index.html";

		[TestMethod]
		public void SetEncoding()
		{
			var site = new Site
			{
				EncodingName = "UTF-8"
			};
			Assert.AreEqual(Encoding.UTF8, site.Encoding);
		}

		[TestMethod]
		public void AddRequests()
		{
			Site site = new Site { EncodingName = "UTF-8", Timeout = 3000 };
			site.ClearStartRequests();
			site.AddStartUrl(Url);
			site.AddStartRequest(new Request(Url, null));
			Assert.IsTrue(site.StartRequests.Contains(new Request(Url, null)));
		}

		[TestMethod]
		public void AddRequestsAsync()
		{
			Site site = new Site { EncodingName = "UTF-8", Timeout = 3000 };
			site.ClearStartRequests();

			Parallel.For(1, 10000, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, i =>
			{
				site.AddStartUrl("http://a.com/" + i);
			});

			Assert.IsTrue(site.StartRequests.Contains(new Request("http://a.com/1", null)));
		}

		[TestMethod]
		public void AddHeaders()
		{
			Site site = new Site { EncodingName = "UTF-8", Timeout = 3000 };
			site.AddHeader(WName, WValue);
			Assert.IsNotNull(site.Headers);
			Assert.IsTrue(site.Headers.Count > 0);
		}
	}
}
