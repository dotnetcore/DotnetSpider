namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// <see cref="IBeforeDownloadHandler"/>
	/// </summary>
	public interface IBeforeDownloadHandler
	{
		void Handle(ref Request request, ISpider spider);
	}
}
