using NLog;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// <see cref="IAfterDownloadCompleteHandler"/>
	/// </summary>
	public abstract class AfterDownloadCompleteHandler : Named, IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		/// <summary>
		/// 处理页面数据、检测下载情况(是否被反爬)、更新Cookie等操作
		/// </summary>
		/// <param name="page"><see cref="Page"/></param>
		/// <param name="spider"><see cref="ISpider"/></param>
		public abstract void Handle(ref Page page, ISpider spider);
	}
}