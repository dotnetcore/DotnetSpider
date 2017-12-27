using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie 注入器的抽象
	/// </summary>
	public abstract class CookieInjector : Named, ICookieInjector
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public virtual void Inject(ISpider spider, bool stopSpider = true)
		{
			if (stopSpider)
			{
				spider.Pause(() =>
				{
					spider.Site.Cookies = GetCookies(spider);
					Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
					spider.Contiune();
				});
			}
			else
			{
				spider.Site.Cookies = GetCookies(spider);
				Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
			}
		}

		protected abstract Cookies GetCookies(ISpider spider);
	}
}
