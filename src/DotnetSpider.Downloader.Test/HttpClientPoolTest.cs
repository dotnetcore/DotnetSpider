//using DotnetSpider.Core.Downloader;
//using DotnetSpider.Core.Infrastructure;
//using DotnetSpider.Core.Proxy;
//using System;
//using System.Collections.Concurrent;
//using System.Threading.Tasks;
//using Xunit;

//namespace DotnetSpider.Core.Test.Downloader
//{
//	public class HttpClientPoolTest
//	{
//		[Fact(DisplayName = "MultiThreadProxy")]
//		public void MultiThreadProxy()
//		{
//			IHttpClientPool pool = new HttpClientPool();
//			var spider = new DefaultSpider();
//			var downloader = new HttpClientDownloader();

//			ConcurrentDictionary<HttpClientEntry, int> tonggi = new ConcurrentDictionary<HttpClientEntry, int>();
//			Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (i) =>
//			{
//				var port = i % 10;
//				var proxy = new UseSpecifiedUriWebProxy(new Uri($"http://192.168.10.1:{port}"));
//				var item = pool.GetHttpClient(proxy.Hash);

//				if (tonggi.ContainsKey(item))
//				{
//					tonggi[item] = tonggi[item] + 1;
//				}
//				else
//				{
//					tonggi.TryAdd(item, 1);
//				}
//			});

//			Assert.Equal(10, tonggi.Count);
//			foreach (var pair in tonggi)
//			{
//				Assert.Equal(100, pair.Value);
//			}
//		}
//	}
//}
