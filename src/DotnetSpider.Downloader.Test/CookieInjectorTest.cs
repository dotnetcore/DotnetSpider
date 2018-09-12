using System;
using System.IO;
using Xunit;

namespace DotnetSpider.Downloader.Test
{
	public class CookieInjectorTest
	{
		[Fact(DisplayName = "InjectCookies")]
		public void InjectCookies()
		{
			var path = "a.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.AppendAllLines(path, new[] { "www.baidu.com" });
			File.AppendAllLines(path, new[] { "a=b;c=d" });
			FileCookieInject inject = new FileCookieInject(path);

			var downloader = new HttpClientDownloader();
			inject.Inject(downloader);
			var cookies = downloader.GetCookies(new Uri("http://www.baidu.com"));
			Assert.Equal("b", cookies["a"].Value);
			Assert.Equal("d", cookies["c"].Value);
		}

	}
}
