using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 爬虫异常
	/// </summary>
	public class SpiderException : Exception
	{
		public SpiderException(string msg) : base(msg) { }

		public SpiderException(string msg, Exception e) : base(msg, e) { }
	}
}
