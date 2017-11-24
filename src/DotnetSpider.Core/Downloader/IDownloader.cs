namespace DotnetSpider.Core.Downloader
{

    /// <summary>
    /// 负责下载HTML，可以实现如HttpDownloader, 浏览器的Downloader(WebDriver), FiddlerDownloader，本地文件Downloader等等
    /// </summary>
	public interface IDownloader : System.IDisposable
	{
		/// <summary>
		/// Downloads web pages and store in Page object.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="spider"></param>
		/// <returns></returns>
		Page Download(Request request, ISpider spider);

		void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler);

		void AddBeforeDownloadHandler(IBeforeDownloadHandler handler);
	}
}
