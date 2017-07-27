namespace DotnetSpider.Core.Downloader
{
	public interface IDownloadCompleteHandler
	{
		bool Handle(ref Page page, ISpider spider);
	}
}
