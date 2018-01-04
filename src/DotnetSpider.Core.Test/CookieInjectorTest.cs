using DotnetSpider.Core.Downloader;
using Xunit;
using System.IO;

namespace DotnetSpider.Core.Test
{

	public class CookieInjectorTest
	{
		[Fact]
		public void InjectCookiesBeforeSpiderRun()
		{
			var path = "a.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.AppendAllLines(path, new[] { "www.baidu.com" });
			File.AppendAllLines(path, new[] { "a=b;c=d" });
			FileCookieInject inject = new FileCookieInject();
			Site site = new Site();
			site.AddStartUrl("http://www.baidu.com");
			DefaultSpider spider = new DefaultSpider("a", site);
			inject.Inject(spider, false);
			var cookies = spider.Cookies.GetCookies("www.baidu.com");
			Assert.Contains(cookies, c => c.Name == "a" && c.Value == "b");
			Assert.Contains(cookies, c => c.Name == "c" && c.Value == "d");
		}
	}
}
