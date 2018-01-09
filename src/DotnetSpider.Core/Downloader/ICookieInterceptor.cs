using System.Net;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie注入器
	/// </summary>
	public interface ICookieInjector
	{
		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		/// <param name="spider">需要注入Cookie的爬虫</param>
		/// <param name="pauseBeforeInject">注入Cookie前是否先暂停爬虫</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void Inject(IDownloader downloader, ISpider spider, bool pauseBeforeInject = true);
	}
}
