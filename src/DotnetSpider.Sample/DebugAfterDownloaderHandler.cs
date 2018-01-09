using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Sample
{
	public class DebugAfterDownloaderHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
		{
		}
	}
}
