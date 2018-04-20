using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Proxy;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class HttpClientPoolTest
	{
		[Fact(DisplayName = "MultiThreadProxy")]
		public void MultiThreadProxy()
		{
			HttpClientPool pool = new HttpClientPool();
			var spider = new DefaultSpider();
			var downloader = new HttpClientDownloader();

			System.Collections.Concurrent.ConcurrentDictionary<HttpClientElement, int> tonggi = new System.Collections.Concurrent.ConcurrentDictionary<HttpClientElement, int>();
			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (i) =>
			{
				var port = i % 10;
				var proxy = new UseSpecifiedUriWebProxy(new Uri($"http://192.168.10.1:{port}"), null, false);
				var item = pool.GetHttpClient(spider, downloader, new System.Net.CookieContainer(), proxy, null);

				if (tonggi.ContainsKey(item))
				{
					tonggi[item] = tonggi[item] + 1;
				}
				else
				{
					tonggi.TryAdd(item, 1);
				}
			});

			Assert.Equal(10, tonggi.Count);
			foreach (var pair in tonggi)
			{
				Assert.Equal(100, pair.Value);
			}
		}
	}
}
