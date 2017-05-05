namespace DotnetSpider.Core
{
	[System.Flags]
	public enum Status
	{
		Init = 1, Running = 2, Stopped = 4, Finished = 8, Exited = 16
	}
}
