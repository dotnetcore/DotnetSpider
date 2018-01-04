using DotnetSpider.Core.Infrastructure;
using NLog;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie 注入器的抽象
	/// </summary>
	public abstract class CookieInjector : Named, ICookieInjector
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫</param>
		public virtual void Inject(ISpider spider, bool pauseBeforeInject = true)
		{
			if (pauseBeforeInject)
			{
				spider.Pause(() =>
				{
					spider.Cookies = GetCookies(spider);
					Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
					spider.Contiune();
				});
			}
			else
			{
				spider.Cookies = GetCookies(spider);
				Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
			}
		}

		/// <summary>
		/// 取得新的Cookies
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <returns>Cookies</returns>
		protected abstract Cookies GetCookies(ISpider spider);
	}
}
