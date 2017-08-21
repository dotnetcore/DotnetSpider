namespace DotnetSpider.Core.Downloader
{
	public interface IAfterDownloadCompleteHandler
	{
		bool Handle(ref Page page, ISpider spider);
	}

	public interface IBeforeDownloadHandler
	{
		bool Handle(ref Request request, ISpider spider);
	}
}
