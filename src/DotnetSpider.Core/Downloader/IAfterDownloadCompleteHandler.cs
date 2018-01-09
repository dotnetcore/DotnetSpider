namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载器完成下载工作后运行的处理工作
	/// </summary>
	public interface IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// 处理页面数据、检测下载情况(是否被反爬)、更新Cookie等操作
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="spider">爬虫</param>
		void Handle(ref Page page, IDownloader downloader, ISpider spider);
	}
}
