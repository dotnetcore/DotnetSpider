using DotnetSpider.Core.Infrastructure;
using NLog;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie 注入器的抽象, 因多线程的原因, 导致某些极限情况会在极短时间内多次尝试注入, 需要设置 FrequencyLimitation来规避
	/// </summary>
	public abstract class CookieInjector : Named, ICookieInjector
	{
		private DateTime _lastInjectTime;

		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		/// <summary>
		/// 重复调用的频率限制(秒)
		/// </summary>
		public int FrequencyLimitation { get; set; } = 15;

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		/// <param name="spider">需要注入Cookie的爬虫</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫</param>
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
					Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
					spider.Contiune();
				});
			}
			else
			{
				foreach (Cookie cookie in GetCookies(spider))
				{
					downloader.AddCookie(cookie);
				}
				Logger.AllLog(spider.Identity, "Inject cookies success.", LogLevel.Info);
			}
		}

		/// <summary>
		/// 取得新的Cookies
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <returns>Cookies</returns>
		protected abstract CookieCollection GetCookies(ISpider spider);

		/// <summary>
		/// 60 秒内重复注入
		/// </summary>
		/// <returns></returns>
		protected bool CheckFrequency()
		{
			var now = DateTime.Now;
			if ((now - _lastInjectTime).TotalSeconds < FrequencyLimitation)
			{
				return false;
			}
			_lastInjectTime = now;
			return true;
		}
	}
}
