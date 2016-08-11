namespace DotnetSpider.Core.Downloader
{
	public interface IDownloadCompleteHandler
	{
		void Handle(Page page);
	}
}
