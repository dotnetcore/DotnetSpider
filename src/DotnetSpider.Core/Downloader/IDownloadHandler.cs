namespace DotnetSpider.Core.Downloader
{

    /// <summary>
    /// 下载完成之后的处理
    /// </summary>
	public interface IAfterDownloadCompleteHandler
	{
        /// <summary>
        /// You can process page data, detect download status(whether is banned) and update Cookie here.
        /// </summary>
        /// <summary>
        /// 处理页面数据、检测下载情况(是否被反爬)、更新Cookie等操作
        /// </summary>
        /// <param name="page"><see cref="Page"/></param>
        /// <param name="downloader">下载器 <see cref="IDownloader"/></param>
        /// <param name="spider"><see cref="ISpider"/></param>
           void Handle(ref Page page, IDownloader downloader, ISpider spider);
    }
    /// <summary>
    /// 下载之前的处理
    /// </summary>
	public interface IBeforeDownloadHandler
	{
        /// <summary>
        /// Pre-process before downloading
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 下载工作的预处理
        /// </summary>
        /// <param name="request">请求信息 <see cref="Request"/></param>
        /// <param name="downloader">下载器 <see cref="IDownloader"/></param>
        /// <param name="spider">爬虫 <see cref="ISpider"/></param>
        void Handle(ref Request request, IDownloader downloader, ISpider spider);
    }
}
