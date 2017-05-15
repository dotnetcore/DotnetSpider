using System.IO;

namespace DotnetSpider.Core.Downloader
{
	public class FileCookieInject : CookieInjector
	{
		protected override Cookies GetCookies(ISpider spider)
		{
			var path = $"{spider.Identity}.cookies";
			if (File.Exists(path))
			{
				var cookie = File.ReadAllText(path);
				return new Cookies
				{
					StringPart = cookie
				};
			}
			else
			{
				return new Cookies();
			}
		}
	}
}
