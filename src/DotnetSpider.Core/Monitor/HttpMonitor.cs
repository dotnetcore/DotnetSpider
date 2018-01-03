using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// HTTP爬虫监控状态上报
	/// 在配置文件件添加了serviceUrl才会生效
	/// </summary>
	public class HttpMonitor : NLogMonitor
	{
		private readonly string _apiUrl;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="apiUrl">上报状态的WebApi入口</param>
		public HttpMonitor(string apiUrl)
		{
			_apiUrl = apiUrl;
		}

		public override void Report(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			base.Report(identity, taskId, status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);

			if (!Env.EnterpiseService)
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
				NodeId = NodeId.Id,
				Status = status,
				Success = success,
				Thread = threadNum,
				Total = total
			});
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			NetworkCenter.Current.Execute("status", () =>
			{
				HttpSender.Client.PostAsync(_apiUrl, content).Wait();
			});
		}
	}
}
