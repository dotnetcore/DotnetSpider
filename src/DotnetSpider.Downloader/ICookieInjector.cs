using System;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// Cookie注入器
	/// </summary>
	public interface ICookieInjector
	{
		/// <summary>
		/// 执行注入 Cookie
		/// </summary>
		/// <param name="downloader">下载器</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void Inject(IDownloader downloader);
	}
}
