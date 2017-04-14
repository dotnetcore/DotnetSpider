using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	public abstract class CookieInjector : Named, ICookieInjector
	{
		public virtual void Inject(ISpider spider, bool stopSpider = true)
		{
			if (stopSpider)
			{
				spider.Pause();
			}
			spider.Site.Cookies = GetCookies(spider.Site);
			spider.Log("注入 Cookies 成功。", LogLevel.Info);

			if (stopSpider)
			{
				spider.Contiune();
			}
		}

		protected abstract Cookies GetCookies(Site site);
	}
}
