namespace DotnetSpider.Core.Downloader
{
	public interface IBeforeDownloadHandler
	{
		void Handle(Request request, ISpider spider);
	}
}
