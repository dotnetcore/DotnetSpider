using DotnetSpider.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class CookieInjectorTest
	{
		[TestMethod]
		public void InjectCookiesBeforeSpiderRun()
		{
			var path = "a.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllText(path, "a=b&c=d");
			FileCookieInject inject = new FileCookieInject();
			Site site = new Site();
			site.AddStartUrl("http://www.baidu.com");
			DefaultSpider spider = new DefaultSpider("a", site);
			inject.Inject(spider, false);
			Assert.AreEqual("a=b&c=d", site.Cookies.ToString());
		}
	}
}
