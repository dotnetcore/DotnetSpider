using DotnetSpider.Common;
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

		public IControllable Controllable { get; }

		/// <summary>
		/// Mininum interval between injections (in second).
		/// </summary>
		/// <summary>
		/// 重复调用的频率限制(秒)
		/// </summary>
		public int FrequencyLimitation { get; set; } = 15;

		public CookieInjector(IControllable controllable)
		{
			Controllable = controllable;
		}

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		/// <param name="pauseBeforeInject">是否停止程序再注入 Cookie: 在程序第一次下载的时候不需要调用暂停</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Inject(IDownloader downloader, bool pauseBeforeInject)
		{
			if (!CheckFrequency())
			{
				return;
			}
			if (pauseBeforeInject)
			{
				Controllable.Pause(() =>
				{
					foreach (Cookie cookie in GetCookies(Controllable))
					{
						downloader.AddCookie(cookie);
					}
					downloader.Logger?.Information("Inject cookies success.");
					Controllable.Contiune();
				});
			}
			else
			{
				foreach (Cookie cookie in GetCookies(Controllable))
				{
					downloader.AddCookie(cookie);
				}
				downloader.Logger?.Information("Inject cookies success.");
			}
		}

		/// <summary>
		/// Obtain new Cookies.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 取得新的Cookies
		/// </summary>
		/// <param name="controllable">可控制程序 <see cref="IControllable"/></param>
		/// <returns>Cookies <see cref="CookieCollection"/></returns>
		protected abstract CookieCollection GetCookies(IControllable controllable);

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
