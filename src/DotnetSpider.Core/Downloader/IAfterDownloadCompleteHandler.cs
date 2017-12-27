namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// <see cref="IAfterDownloadCompleteHandler"/>
	/// </summary>
	public interface IAfterDownloadCompleteHandler
	{
		void Handle(ref Page page, ISpider spider);
	}
}
