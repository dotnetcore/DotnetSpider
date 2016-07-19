using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Monitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Extension.Monitor
{
	public class HttpMonitor : IMonitorService
	{
		private HttpClient _client = new HttpClient();
		private AutomicLong _postCounter = new AutomicLong(0);
		private string _server;

		public HttpMonitor(string server)
		{
			_server = server;
		}

		public bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(_server) && !string.IsNullOrWhiteSpace(_server);
			}
		}

		public void Dispose()
		{
			while (true)
			{
				if (_postCounter.Value == 0)
				{
					break;
				}

				Thread.Sleep(500);
			}
		}

		public void SaveStatus(dynamic spider)
		{
			var status = new
			{
				Message = new
				{
					Error = spider.Scheduler.GetErrorRequestsCount(),
					Left = spider.Scheduler.GetLeftRequestsCount(),
					Status = spider.StatusCode,
					Success = spider.Scheduler.GetSuccessRequestsCount(),
					Thread = spider.ThreadNum,
					Total = spider.Scheduler.GetTotalRequestsCount()
				},
				Name = spider.Identity,
				Machine = SystemInfo.HostName,
				UserId = spider.UserId,
				TaskGroup = spider.TaskGroup,
				Timestamp = DateTime.Now
			};
			_postCounter.Inc();
			var task = _client.PostAsync(_server, new StringContent(JsonConvert.SerializeObject(status), Encoding.UTF8, "application/json"));

			task.ContinueWith((t) =>
			{
				_postCounter.Dec();
			});
		}
	}
}
