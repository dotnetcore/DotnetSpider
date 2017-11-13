using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public class HttpExecuteRecord : IExecuteRecord
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		public ISpider Spider { get; private set; }

		public HttpExecuteRecord(ISpider spider)
		{
			Spider = spider;
		}

		public bool Add()
		{
			var json = JsonConvert.SerializeObject(new
			{
				TaskId = 1
			});
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			for (int i = 0; i < 10; ++i)
			{
				try
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = HttpSender.Client.PostAsync(Env.HttpAddExecuteRecordUrl, content).Result;
						response.EnsureSuccessStatusCode();
					});
					return true;
				}
				catch (Exception ex)
				{
					Logger.Error($"Add execute record: {ex}");
					Thread.Sleep(5000);
				}
			}
			return false;
		}

		public void Remove()
		{
			var json = JsonConvert.SerializeObject(new
			{
				TaskId = Spider.TaskId
			});
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			for (int i = 0; i < 10; ++i)
			{
				try
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = HttpSender.Client.PostAsync(Env.HttpRemoveExecuteRecordUrl, content).Result;
						response.EnsureSuccessStatusCode();
					});
					break;
				}
				catch (Exception ex)
				{
					Logger.Error($"Remove execute record failed: {ex}");
					Thread.Sleep(5000);
				}
			}
		}
	}
}
