using DotnetSpider.Core;

namespace DotnetSpider.Extension.Redial
{
	public class RedialException : SpiderException
	{
		public RedialException(string message) : base(message)
		{
		}
	}
}
