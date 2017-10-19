namespace DotnetSpider.Core.Downloader
{
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