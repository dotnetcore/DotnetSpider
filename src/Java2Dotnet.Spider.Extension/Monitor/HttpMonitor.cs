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

		public HttpMonitor()
		{
			_server = ConfigurationManager.Get("statusHttpServer");
		}

		public bool IsEnabled
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

		public void Watch(SpiderStatus status)
		{
			var tmp = new
			{
				Message = new
				{
					Error = status.Error,
					Left = status.Left,
					Status = status.Code,
					Success = status.Success,
					Thread = status.ThreadNum,
					Total = status.Total
				},
				Name = status.Identity,
				Machine = status.Machine,
				UserId = status.UserId,
				TaskGroup = status.TaskGroup,
				Timestamp = status.Timestamp
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
