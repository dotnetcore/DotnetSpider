using Xunit;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Test
{

	public class SiteTest
	{
		private string _wName = "WebSite";
		private string _wValue = "12580emall";
		private string _url = @"http://www.12580emall.com/emall/mall/index.html";

		[Fact]
		public void SetEncoding()
		{
			var site = new Site
			{
				EncodingName = "UTF-8"
			};
			Assert.Equal(Encoding.UTF8, site.Encoding);
		}

		[Fact]
		public void AddRequests()
		{
			Site site = new Site { EncodingName = "UTF-8", Timeout = 3000 };
			site.ClearStartRequests();
			site.AddStartUrl(_url);
			site.AddStartRequest(new Request(_url, null));
			Assert.Contains(new Request(_url, null), site.StartRequests);
		}

		[Fact]
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

			Assert.Contains(new Request("http://a.com/1", null), site.StartRequests);
		}

		[Fact]
		public void AddHeaders()
		{
			Site site = new Site { EncodingName = "UTF-8", Timeout = 3000 };
			site.AddHeader(_wName, _wValue);
			Assert.NotNull(site.Headers);
			Assert.True(site.Headers.Count > 0);
		}
	}
}
