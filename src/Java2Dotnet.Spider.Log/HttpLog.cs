using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Log
{
	public class HttpLog : ILogService
	{
		private string Server { get; set; }
		private HttpClient Client = new HttpClient();
		private int Counter = 0;

		public HttpLog(string server)
		{
			Server = server;
		}

		public void Error(dynamic message)
		{
			WriteToServer("[错误] " + message);
		}

		public void Error(dynamic message, Exception e)
		{
			WriteToServer("[错误] " + message + ": " + e);
		}

		public void Info(dynamic message)
		{
			WriteToServer("[信息] " + message);
		}

		public void Info(dynamic message, Exception e)
		{
			WriteToServer("[信息] " + message + ": " + e);
		}

		public void Warn(dynamic message)
		{
			WriteToServer("[警告] " + message);
		}

		public void Warn(dynamic message, Exception e)
		{
			WriteToServer("[警告]" + message + ": " + e);
		}

		private void WriteToServer(string log)
		{
			lock (this)
			{
				++Counter;
			}

			var task = Client.PostAsync(Server, new StringContent(log.ToString().Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r"), Encoding.UTF8, "application/json"));

			task.ContinueWith((t) =>
			{
				lock (this)
				{
					--Counter;
				}
			});
		}

		public void Dispose()
		{
			while (true)
			{
				if (Counter > 0)
				{
					Thread.Sleep(100);
				}
				else
				{
					break;
				}
			}
		}
	}
}
