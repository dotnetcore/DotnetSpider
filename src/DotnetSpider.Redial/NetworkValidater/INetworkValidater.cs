namespace DotnetSpider.Redial.NetworkValidater
{
	public interface INetworkValidater
	{
		int MaxWaitTime { get; set; }
		bool Wait();
	}
}
