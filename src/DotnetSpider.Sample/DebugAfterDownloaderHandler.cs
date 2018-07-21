using DotnetSpider.Common;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample
{
	public class DebugAfterDownloaderHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Response response, IDownloader downloader)
		{
		}
	}
}
