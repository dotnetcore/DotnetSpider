using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Common.Test
{
	public class SiteTest
	{
		private string _wName = "WebSite";
		private string _wValue = "12580emall";
		private string _url = @"http://www.12580emall.com/emall/mall/index.html";


		[Fact(DisplayName = "Site_AddRequests")]
		public void AddRequests()
		{
			Site site = new Site { EncodingName = "UTF-8" };
			site.Requests.Clear();
			site.AddRequests(_url);
			site.AddRequests(new Request(_url, null));
			Assert.Contains(site.Requests, r => r.Url == _url);
		}

		[Fact(DisplayName = "Site_AddRequestsAsync")]
		public void AddRequestsAsync()
		{
			Site site = new Site { EncodingName = "UTF-8" };
			site.Requests.Clear();

			Parallel.For(1, 10000, parallelOptions: new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
			}, body: i =>
			{
				site.AddRequests("http://a.com/" + i);
			});
			Assert.Contains(site.Requests, r => r.Url == "http://a.com/1");
		}

		[Fact(DisplayName = "Site_AddHeaders")]
		public void AddHeaders()
		{
			Site site = new Site { EncodingName = "UTF-8" };
			site.AddHeader(_wName, _wValue);
			Assert.NotNull(site.Headers);
			Assert.True(site.Headers.Count > 0);
		}
	}
}
