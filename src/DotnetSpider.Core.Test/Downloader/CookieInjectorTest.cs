using DotnetSpider.Core.Downloader;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Core.Test.Downloader
{
	public class CookieInjectorTest
	{
		[Fact(DisplayName = "FileCookieInject")]
		public void FileCookieInject()
		{
			if (Environment.GetEnvironmentVariable("TRAVIS") == "1")
			{
				return;
			}
			FileCookieInject cookieInject = new FileCookieInject("source\\test.cookies");
			var spider = new DefaultSpider();
			cookieInject.Inject(spider.Downloader, spider);
			var cookies = spider.Downloader.GetCookies(new Uri("http://baidu.com"));
			Assert.Equal("b", cookies["a"].Value);
			Assert.Equal("e", cookies["c"].Value);
		}

		[Fact(DisplayName = "FileCookieInject_FileNotExists")]
		public void FileCookieInject_FileNotExists()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				FileCookieInject cookieInject = new FileCookieInject("notexists.cookies");
			});
		}
	}
}
