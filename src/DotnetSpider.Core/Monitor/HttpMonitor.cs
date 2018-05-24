using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Monitor
{
	public class HttpMonitor : NLogMonitor
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

        public HttpMonitor(string taskId, string identity) : base(taskId, identity)
        {
       
		}

		public override void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			base.Report(status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);

			var json = JsonConvert.SerializeObject(new SpiderStatus
			{
				TaskId = _taskId,
				AvgDownloadSpeed = avgDownloadSpeed,
				AvgPipelineSpeed = avgPipelineSpeed,
				AvgProcessorSpeed = avgProcessorSpeed,
				Error = error,
				Identity = _identity,
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
				HttpSender.Client.PostAsync(Env.HttpStatusUrl, content).Wait();
			});
		}
	}
}
