using DotnetSpider.Common;
using System;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Handler that regularly update cookies.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 定时更新Cookie的处理器
	/// </summary>
	public class TimingUpdateCookieHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;
		private readonly int _interval;
		private DateTime _next;

		/// <summary>
		/// Construct a <see cref="TimingUpdateCookieHandler"/> instance.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="interval">间隔时间 interval time in second</param>
		/// <param name="injector">Cookie注入器 <see cref="ICookieInjector"/></param>
		/// <exception cref="ArgumentException">dueTime should be large than 0.</exception>
		public TimingUpdateCookieHandler(int interval, ICookieInjector injector)
		{
			if (interval <= 0)
			{
				throw new ArgumentException("interval should be large than 0.");
			}

			_cookieInjector = injector ?? throw new DownloaderException($"{nameof(injector)} should not be null.");
			_next = DateTime.Now.AddSeconds(_interval);
			_interval = interval;
		}

		/// <summary>
		/// Update cookies regularly.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 定时更新Cookie
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (DateTime.Now > _next)
			{
				_next = DateTime.Now.AddSeconds(_interval);
				_cookieInjector.Inject(downloader, true);
			}
		}
	}
}
