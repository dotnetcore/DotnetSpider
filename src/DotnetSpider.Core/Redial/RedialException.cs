namespace DotnetSpider.Core.Redial
{
	/// <summary>
	/// 拨号异常
	/// </summary>
	public class RedialException : SpiderException
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="message">异常信息</param>
		public RedialException(string message) : base(message)
		{
		}
	}
}
