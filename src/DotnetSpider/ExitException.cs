namespace DotnetSpider
{
	public sealed class ExitException : SpiderException
	{
		public ExitException(string msg) : base(msg)
		{
		}
	}
}
