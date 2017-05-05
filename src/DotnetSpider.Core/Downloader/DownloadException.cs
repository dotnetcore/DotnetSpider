namespace DotnetSpider.Core.Downloader
{
	public class DownloadException : SpiderException
	{
		public DownloadException(string message) : base(message)
		{
		}
	}
}
