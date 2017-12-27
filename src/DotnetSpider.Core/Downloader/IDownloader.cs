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

		void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler);

		void AddBeforeDownloadHandler(IBeforeDownloadHandler handler);
	}
}
