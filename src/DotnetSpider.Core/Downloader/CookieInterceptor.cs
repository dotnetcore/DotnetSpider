using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	public abstract class CookieInjector : Named, ICookieInjector
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();

		public virtual void Inject(ISpider spider, bool stopSpider = true)
		{
			if (stopSpider)
			{
				spider.Pause(() =>
				{
					spider.Site.Cookies = GetCookies(spider);
					Logger.MyLog(spider.Identity, "注入 Cookies 成功.", LogLevel.Info);
					spider.Contiune();
				});
			}
			else
			{
				spider.Site.Cookies = GetCookies(spider);
				Logger.MyLog(spider.Identity, "注入 Cookies 成功.", LogLevel.Info);
			}
		}

		protected abstract Cookies GetCookies(ISpider spider);
	}
}
