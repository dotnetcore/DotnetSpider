using DotnetSpider.Core.Downloader;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class CookieInjectorTest
	{
		[Fact]
		public void FileCookieInject()
		{
			FileCookieInject cookieInject = new FileCookieInject("Downloader\\test.cookies");
			var spider = new DefaultSpider();
			cookieInject.Inject(spider.Downloader, spider);
			var cookies = spider.Downloader.GetCookies(new Uri("http://baidu.com"));
			Assert.Equal("b", cookies["a"].Value);
			Assert.Equal("e", cookies["c"].Value);
		}

		[Fact]
		public void FileCookieInject_FileNotExists()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				FileCookieInject cookieInject = new FileCookieInject("notexists.cookies");
			});
		}
	}
}
