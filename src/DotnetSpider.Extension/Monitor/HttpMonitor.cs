using DotnetSpider.Core.Common;
using DotnetSpider.Core.Monitor;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace DotnetSpider.Extension.Monitor
{
	public class HttpMonitor : IMonitorService
	{
		private readonly HttpClient _client = new HttpClient();
		private readonly AutomicLong _postCounter = new AutomicLong(0);
		private readonly string _server;

		public HttpMonitor()
		{
			_server = Configuration.GetValue("statusHttpServer");
		}

		public bool IsEnabled => !string.IsNullOrEmpty(_server) && !string.IsNullOrWhiteSpace(_server);

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
					status.Error,
					status.Left,
					Status = status.Code,
					status.Success,
					Thread = status.ThreadNum,
					status.Total
				},
				Name = status.Identity,
				status.Machine,
				status.UserId,
				status.TaskGroup,
				status.Timestamp
			};
			_postCounter.Inc();
			var task = _client.PostAsync(_server, new StringContent(JsonConvert.SerializeObject(tmp), Encoding.UTF8, "application/json"));

			task.ContinueWith(t =>
			{
				_postCounter.Dec();
			});
		}
	}
}
