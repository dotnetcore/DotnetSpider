using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	public class FileCookieInject : CookieInjector
	{
		protected override Cookies GetCookies(Site site)
		{
			var path = $"{site.Domain}.cookies";
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
