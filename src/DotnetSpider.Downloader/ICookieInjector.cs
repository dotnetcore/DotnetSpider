using DotnetSpider.Common;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// Cookie注入器
	/// </summary>
	public interface ICookieInjector
	{
		IControllable Controllable { get; }

		/// <summary>
		/// 执行注入Cookie的操作
		/// </summary>
		/// <param name="downloader">下载器</param>
		/// <param name="pauseBeforeInject">是否停止程序再注入 Cookie: 在程序第一次下载的时候不需要调用暂停</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void Inject(IDownloader downloader, bool pauseBeforeInject);
	}
}
