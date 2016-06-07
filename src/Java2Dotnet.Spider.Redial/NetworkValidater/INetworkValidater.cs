namespace Java2Dotnet.Spider.Redial.NetworkValidater
{
	public interface INetworkValidater
	{
		int MaxWaitTime { get; set; }
		bool Wait();
	}
}
