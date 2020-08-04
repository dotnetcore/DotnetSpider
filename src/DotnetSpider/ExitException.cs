namespace DotnetSpider
{
	public class ExitException : SpiderException
	{
		public ExitException(string msg) : base(msg)
		{
		}
	}
}
