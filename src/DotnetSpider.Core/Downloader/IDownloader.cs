namespace DotnetSpider.Core.Downloader
{
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

		IDownloader Clone(ISpider spider);
	}
}
