using NLog;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Downloader
{
	public abstract class AfterDownloadCompleteHandler : Named, IAfterDownloadCompleteHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public abstract void Handle(ref Page page, ISpider spider);
	}
}