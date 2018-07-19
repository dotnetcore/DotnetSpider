using DotnetSpider.Common;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// <see cref="IAfterDownloadCompleteHandler"/>
	/// </summary>
	public abstract class AfterDownloadCompleteHandler : Named, IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// You can process page data, detect download status(whether is banned) and update Cookie here.
		/// </summary>
		/// <summary>
		/// 处理页面数据、检测下载情况(是否被反爬)、更新Cookie等操作
		/// </summary>
		/// <param name="response"><see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public abstract void Handle(ref Response response, IDownloader downloader);
	}
}
