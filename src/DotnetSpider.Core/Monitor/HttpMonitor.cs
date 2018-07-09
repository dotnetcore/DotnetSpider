using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;

namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// HTTP爬虫监控状态上报
	/// 在配置文件件添加了serviceUrl才会生效
	/// </summary>
	public class HttpMonitor : LogMonitor
	{
		/// <summary>
		/// 上报爬虫状态
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="taskId">任务编号</param>
		/// <param name="status">爬虫状态: 运行、暂停、退出、完成</param>
		/// <param name="left">剩余的目标链接数</param>
		/// <param name="total">总的需要采集的链接数</param>
		/// <param name="success">成功采集的链接数</param>
		/// <param name="error">采集出错的链接数</param>
		/// <param name="avgDownloadSpeed">平均下载一个链接需要的时间(豪秒)</param>
		/// <param name="avgProcessorSpeed">平均解析一个页面需要的时间(豪秒)</param>
		/// <param name="avgPipelineSpeed">数据管道处理一次数据结果需要的时间(豪秒)</param>
		/// <param name="threadNum">爬虫线程数</param>
		public override void Flush(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			base.Flush(identity, taskId, status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);

			if (!Env.HubService)
			{
				return;
			}
			var json = JsonConvert.SerializeObject(new SpiderStatus
			{
				TaskId = taskId,
				AvgDownloadSpeed = avgDownloadSpeed,
				AvgPipelineSpeed = avgPipelineSpeed,
				AvgProcessorSpeed = avgProcessorSpeed,
				Error = error,
				Identity = identity,
				Left = left,
				NodeId = Env.NodeId,
				Status = status,
				Success = success,
				Thread = threadNum,
				Total = total
			});
			HubService.HttpStatus(json);
		}
	}
}
