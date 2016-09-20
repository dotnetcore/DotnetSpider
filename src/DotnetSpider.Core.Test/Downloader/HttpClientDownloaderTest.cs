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
			try
			{
				downloader.Download(new Request("http://localhost/abcasdfasdfasdfas", 0, null), spider);
			}
			catch (Exception e)
			{
				bool r = e.Message == "Response status code does not indicate success: 503 (One or more errors occurred. (An error occurred while sending the request.))." || e.Message == "响应状态代码不指示成功: 503 (发生一个或多个错误。)。";
				Assert.True(r);
			}
			Stopwatch watch = new Stopwatch();
			watch.Start();
			try
			{
				downloader.Download(new Request("http://google.com/", 0, null), spider);
			}
			catch (Exception e)
			{
				bool r = e.Message == "Response status code does not indicate success: 408 (Request Timeout)." || e.Message == "响应状态代码不指示成功: 408 (Request Timeout)。";
				Assert.True(r);
			}
			watch.Stop();
			Assert.True(watch.ElapsedMilliseconds > 5000);
			Assert.True(watch.ElapsedMilliseconds < 6000);
		}

		/// <summary>
		/// 手动执行此测试脚本，运行结束后用netstat -ano 查看端口占用情况。只会占用一个就对了。如果
		/// </summary>
		[Fact]
		public void Ports()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			for (int i = 0; i < 100; i++)
			{
				downloader.Download(new Request("http://www.163.com", 0, null), spider);
			}
		}
	}
}
