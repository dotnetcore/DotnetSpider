namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Exception that <see cref="IDownloader"/> throws.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 下载器抛出的异常
	/// </summary>
	public class DownloadException : SpiderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		public DownloadException() : base("Download Exception")
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="message">异常信息 Error message</param>
		public DownloadException(string message) : base(message)
		{
		}
	}
}