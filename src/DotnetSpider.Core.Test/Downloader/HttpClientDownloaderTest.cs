using System;
using System.Diagnostics;
using DotnetSpider.Core.Downloader;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class HttpClientDownloaderTest
	{
		[Fact]
		public void Timeout()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });
			downloader.Download(new Request("http://www.163.com", 0, null), spider);
			Stopwatch watch = new Stopwatch();
			watch.Start();
			try
			{
				downloader.Download(new Request("http://www.xbxbxbxbxbxbxbxbxbxbxbxbxbxbxbxb.com", 0, null), spider);
			}
			catch (Exception e)
			{
				Assert.Equal("Response status code does not indicate success: 408 (Request Timeout).", e.Message);
			}
			watch.Stop();
			Assert.True(watch.ElapsedMilliseconds > 5000);
			Assert.True(watch.ElapsedMilliseconds < 6000);
		}
	}
}
