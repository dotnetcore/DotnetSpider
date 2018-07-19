using System;

namespace DotnetSpider.Downloader
{
	public class DownloaderException : Exception
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		public DownloaderException(string msg) : base(msg) { }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		/// <param name="e">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public DownloaderException(string msg, Exception e) : base(msg, e) { }
	}
}
