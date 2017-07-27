using System;
namespace DotnetSpider.Core.Monitor
{
	public interface IMonitor : IDisposable
	{
		void Report(
			string status,
			long left,
			long total,
			long success,
			long error,
			long avgDownloadSpeed,
			long avgProcessorSpeed,
			long avgPipelineSpeed,
			int threadNum
			);

		string Identity { get; set; }
	}
}
