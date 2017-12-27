namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载器抛出的异常
	/// </summary>
	public class DownloadException : SpiderException
	{
		public DownloadException() : base("Download Exception")
		{
		}

		public DownloadException(string message) : base(message)
		{
		}
	}
}