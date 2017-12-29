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

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="spider">需要注入Cookie的爬虫</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫</param>
		public virtual void Inject(ISpider spider, bool pauseBeforeInject = true)
		{
			if (pauseBeforeInject)
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
