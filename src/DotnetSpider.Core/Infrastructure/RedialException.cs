namespace DotnetSpider.Core.Infrastructure
{
	public class RedialException : SpiderException
	{
		public RedialException(string message) : base(message)
		{
		}
	}
}
