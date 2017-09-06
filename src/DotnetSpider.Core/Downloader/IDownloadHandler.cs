namespace DotnetSpider.Core.Downloader
{
	public interface IAfterDownloadCompleteHandler
	{
		void Handle(ref Page page, ISpider spider);
	}

	public interface IBeforeDownloadHandler
	{
		void Handle(ref Request request, ISpider spider);
	}
}
