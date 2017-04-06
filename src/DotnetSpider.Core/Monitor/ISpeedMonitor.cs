namespace DotnetSpider.Core.Monitor
{
	public interface ISpeedMonitor
	{
		long AvgDownloadSpeed { get; }
		long AvgProcessorSpeed { get; }
		long AvgPipelineSpeed { get; }
	}
}
