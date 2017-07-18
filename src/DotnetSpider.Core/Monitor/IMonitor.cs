using System;
namespace DotnetSpider.Core.Monitor
{
	public interface IMonitor : IDisposable
	{
		void Report(string identity,
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

		bool IsEnabled { get; }
	}
}
