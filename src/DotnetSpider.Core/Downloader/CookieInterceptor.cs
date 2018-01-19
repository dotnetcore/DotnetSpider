using DotnetSpider.Core.Infrastructure;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// The abstraction of <see cref="ICookieInjector"/>.
	/// In multi-thread situations, there will be multi-trying of injecting in short interval, need to set <see cref="FrequencyLimitation"/> to avoid it.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// Cookie 注入器的抽象, 因多线程的原因, 导致某些极限情况会在极短时间内多次尝试注入, 需要设置 FrequencyLimitation来规避
	/// </summary>
	public abstract class CookieInjector : Named, ICookieInjector
	{
		private DateTime _lastInjectedTime;

		/// <summary>
		/// Log interface
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// Mininum interval between injections (in second).
		/// </summary>
		/// <summary>
		/// 重复调用的频率限制(秒)
		/// </summary>
		public int FrequencyLimitation { get; set; } = 15;

		/// <summary>
		/// Inject cookie here.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		/// <param name="spider">需要注入Cookie的爬虫 <see cref="ISpider"/> that need to be injected.</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫 Whether to pause <see cref="ISpider"/> before injection.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void Inject(IDownloader downloader, ISpider spider, bool pauseBeforeInject = true)
		{
			if (!CheckFrequency())
			{
				return;
			}

			if (pauseBeforeInject)
			{
				spider.Pause(() =>
				{
					foreach (Cookie cookie in GetCookies(spider))
					{
						downloader.AddCookie(cookie);
					}
					Logger.Log(spider.Identity, "Inject cookies success.", Level.Info);
					spider.Contiune();
				});
			}
			else
			{
				foreach (Cookie cookie in GetCookies(spider))
				{
					downloader.AddCookie(cookie);
				}
				Logger.Log(spider.Identity, "Inject cookies success.", Level.Info);
			}
		}

		/// <summary>
		/// Obtain new Cookies.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 取得新的Cookies
		/// </summary>
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <returns>Cookies <see cref="CookieCollection"/></returns>
		protected abstract CookieCollection GetCookies(ISpider spider);

		/// <summary>
		/// Check injection frenquency.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 60 秒内重复注入
		/// </summary>
		/// <returns>return true if reach the limit.</returns>
		protected bool CheckFrequency()
		{
			var now = DateTime.Now;
			if ((now - _lastInjectedTime).TotalSeconds < FrequencyLimitation)
			{
				return false;
			}
			_lastInjectedTime = now;
			return true;
		}
	}
}
