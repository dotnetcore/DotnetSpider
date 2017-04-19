using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Downloader
{
	[TestClass]
	public class HttpClientDownloaderTest
	{
		//[TestMethod]
		//public void Timeout()
		//{
		//	HttpClientDownloader downloader = new HttpClientDownloader();
		//	DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });
		//	downloader.Download(new Request("http://www.163.com", null), spider);
		//	try
		//	{
		//		downloader.Download(new Request("http://localhost/abcasdfasdfasdfas", null), spider);
		//		throw new Exception("Test Failed");
		//	}
		//	catch (SpiderException e)
		//	{
		//		Assert.IsNotNull(e);
		//	}
		//	Stopwatch watch = new Stopwatch();
		//	watch.Start();
		//	try
		//	{
		//		downloader.Download(new Request("http://google.com/", null), spider);
		//	}
		//	catch (SpiderException e)
		//	{
		//		Assert.IsNotNull(e);
		//	}
		//	watch.Stop();
		//	Assert.IsTrue(watch.ElapsedMilliseconds > 5000);
		//	Assert.IsTrue(watch.ElapsedMilliseconds < 6000);
		//}

		/// <summary>
		/// 手动执行此测试脚本，运行结束后用netstat -ano 查看端口占用情况。只会占用一个就对了。如果
		/// </summary>
		[TestMethod]
		public void Ports()
		{
			HttpClientDownloader downloader = new HttpClientDownloader();
			DefaultSpider spider = new DefaultSpider("abcd", new Site { Timeout = 5000 });

			for (int i = 0; i < 100; i++)
			{
				downloader.Download(new Request("http://www.163.com", null), spider);
			}
		}

		[TestMethod]
		public void SetContentType()
		{
			Site site1 = new Site
			{
				Headers = new Dictionary<string, string>()
				{
					{"Content-Type","abcd" }
				}
			};
			Site site2 = new Site
			{
				Headers = new Dictionary<string, string>()
				{
					{"ContentType","abcd" }
				}
			};
			HttpClientDownloader downloader = new HttpClientDownloader();
			downloader.Download(new Request("http://baidu.com", null), new DefaultSpider("test", site1));

			downloader.Download(new Request("http://baidu.com", null), new DefaultSpider("test", site2));
		}
	}
}
