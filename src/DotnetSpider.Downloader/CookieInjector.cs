using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// The abstraction of <see cref="ICookieInjector"/>.
	/// In multi-thread situations, there will be multi-trying of injecting in short interval, need to set <see cref="FrequencyLimitation"/> to avoid it.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// Cookie 注入器的抽象, 因多线程的原因, 导致某些极限情况会在极短时间内多次尝试注入, 需要设置 FrequencyLimitation来规避
	/// </summary>
	public abstract class CookieInjector : ICookieInjector
	{
		private DateTime _lastInjectedTime;
		private readonly Action _before;
		private readonly Action _after;

		/// <summary>
		/// Mininum interval between injections (in second).
		/// </summary>
		/// <summary>
		/// 重复调用的频率限制(秒)
		/// </summary>
		public int FrequencyLimitation { get; set; } = 15;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="before">在注入 Cookie 前需要执行的操作, 如暂停爬虫</param>
		/// <param name="after">在注入 Cookie 之后需要执行的操作, 如启动爬虫</param>
		public CookieInjector(Action before, Action after)
		{
			_before = before;
			_after = after;
		}

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Inject(IDownloader downloader)
		{
			if (!CheckFrequency())
			{
				return;
			}

			_before?.Invoke();

			foreach (Cookie cookie in GetCookies())
			{
				downloader.AddCookie(cookie);
			}
			_after?.Invoke();
		}

		/// <summary>
		/// Obtain new Cookies.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 取得新的Cookies
		/// </summary>
		/// <returns>Cookies <see cref="CookieCollection"/></returns>
		protected abstract CookieCollection GetCookies();

		/// <summary>
		/// Check injection frequency.
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
