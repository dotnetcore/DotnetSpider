namespace DotnetSpider.Core.Downloader
{
	public abstract class BeforeDownloadHandler : Named, IBeforeDownloadHandler
	{
		public abstract void Handle(ref Request request, ISpider spider);
	}
}
