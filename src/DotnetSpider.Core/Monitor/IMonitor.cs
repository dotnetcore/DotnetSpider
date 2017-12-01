namespace DotnetSpider.Core.Monitor
{
	public interface IMonitor
	{
		void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum);
	}
}
