namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载工作的预处理, 可以在执行下载前替换关键信息: 如修正PostBody
	/// </summary>
	public abstract class BeforeDownloadHandler : Named, IBeforeDownloadHandler
	{
		/// <summary>
		/// 下载工作的预处理
		/// </summary>
		/// <param name="request">请求信息</param>
		/// <param name="spider">爬虫<see cref="ISpider"/></param>
		public abstract void Handle(ref Request request, IDownloader downloader, ISpider spider);
	}
}
