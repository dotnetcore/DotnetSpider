namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 下载器抛出的异常
	/// </summary>
	public class DownloadException : SpiderException
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DownloadException() : base("Download Exception")
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="message">异常信息</param>
		public DownloadException(string message) : base(message)
		{
		}
	}
}