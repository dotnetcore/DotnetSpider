using System;

namespace DotnetSpider.Downloader
{
	public class BypassedDownloaderException : DownloaderException
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		public BypassedDownloaderException(string msg) : base(msg) { }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		/// <param name="e">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public BypassedDownloaderException(string msg, Exception e) : base(msg, e) { }
	}
}
