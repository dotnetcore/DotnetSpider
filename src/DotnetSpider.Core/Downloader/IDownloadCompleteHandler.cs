namespace DotnetSpider.Core.Downloader
{
	public interface IDownloadCompleteHandler
	{
		bool Handle(Page page, ISpider spider);
	}
}
