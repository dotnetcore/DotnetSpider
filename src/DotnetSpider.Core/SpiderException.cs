using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 爬虫异常
	/// </summary>
	public class SpiderException : Exception
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		public SpiderException(string msg) : base(msg) { }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="msg">异常信息</param>
		/// <param name="e">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public SpiderException(string msg, Exception e) : base(msg, e) { }
	}
}
