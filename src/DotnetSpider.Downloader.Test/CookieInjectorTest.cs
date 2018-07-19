using DotnetSpider.Common;
using System;
using System.IO;
using Xunit;

namespace DotnetSpider.Downloader.Test
{
	public class CookieInjectorTest
	{
		[Fact(DisplayName = "InjectCookiesBeforeSpiderRun")]
		public void InjectCookiesBeforeSpiderRun()
		{
			var path = "a.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.AppendAllLines(path, new[] { "www.baidu.com" });
			File.AppendAllLines(path, new[] { "a=b;c=d" });
			FileCookieInject inject = new FileCookieInject(path, new App());
			Site site = new Site();
			site.AddRequests("http://www.baidu.com");
			var downloader = new HttpWebRequestDownloader();
			inject.Inject(downloader, true);
			var cookies = downloader.GetCookies(new Uri("http://www.baidu.com"));
			Assert.Equal("b", cookies["a"].Value);
			Assert.Equal("d", cookies["c"].Value);
		}

		class App : IControllable
		{
			public ILogger Logger { get; }

			public void Contiune()
			{

			}

			public void Exit(Action action = null)
			{
				action?.Invoke();
			}

			public void Pause(Action action = null)
			{
				action?.Invoke();
			}
		}
	}
}
