namespace DotnetSpider.Core.Redial
{
	/// <summary>
	/// 拨号异常
	/// </summary>
	public class RedialException : SpiderException
	{
		public RedialException(string message) : base(message)
		{
		}
	}
}
