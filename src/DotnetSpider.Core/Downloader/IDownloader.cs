using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载器接口
	/// </summary>
	public interface IDownloader : System.IDisposable
	{
		/// <summary>
		/// 下载链接内容
		/// </summary>
		/// <param name="request">链接请求</param>
		/// <param name="spider">爬虫接口</param>
		/// <returns>下载内容封装好的页面对象</returns>
		Page Download(Request request, ISpider spider);

		/// <summary>
		/// 添加下载完成后的后续处理操作
		/// </summary>
		/// <param name="handler"><see cref="IAfterDownloadCompleteHandler"/></param>
		void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler);

		/// <summary>
		/// 添加下载操作前的处理操作
		/// </summary>
		/// <param name="handler"><see cref="IBeforeDownloadHandler"/></param>
		void AddBeforeDownloadHandler(IBeforeDownloadHandler handler);

		/// <summary>
		/// 重置Cookie
		/// </summary>
		/// <param name="cookies">Cookies</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void ResetCookies(Cookies cookies);
	}
}
