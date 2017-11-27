namespace DotnetSpider.Core.Downloader
{

    /// <summary>
    /// 下载完成之后的处理
    /// </summary>
	public interface IAfterDownloadCompleteHandler
	{
		void Handle(ref Page page, ISpider spider);
	}
    /// <summary>
    /// 下载之前的处理
    /// </summary>
	public interface IBeforeDownloadHandler
	{
		void Handle(ref Request request, ISpider spider);
	}
}
