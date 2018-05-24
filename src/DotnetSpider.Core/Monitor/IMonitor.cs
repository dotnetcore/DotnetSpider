using System;

namespace DotnetSpider.Core.Monitor
{

    /// <summary>
    /// 显示监控 
    /// </summary>
	public interface IMonitor  
	{ 
		void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum);
	}
}
