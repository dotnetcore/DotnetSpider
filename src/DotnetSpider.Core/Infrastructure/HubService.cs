using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using NLog;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
	public class HubService
	{
		private static NLog.ILogger _nlogger;

		static HubService()
		{
			_nlogger = LogManager.GetLogger("DotnetSpider");
		}

		private static RetryPolicy RetryPolicy = Policy.Handle<Exception>().Retry(5, (ex, count) =>
		{
			_nlogger.Error($"Submit http log failed [{count}]: {ex}");
		});

		public static void HttpLog(string log)
		{
			HttpRequestMessage httpRequestMessage = GenerateHttpRequestMessage(log, Env.HunServiceLogUrl);
			Send(httpRequestMessage);
		}

		public static void HttpStatus(string status)
		{
			HttpRequestMessage httpRequestMessage = GenerateHttpRequestMessage(status, Env.HunServiceStatusApiUrl);
			Send(httpRequestMessage);
		}

		private static HttpRequestMessage GenerateHttpRequestMessage(string data, string api)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, api);
			SetToken(httpRequestMessage);
			httpRequestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
			return httpRequestMessage;
		}

		private static void Send(HttpRequestMessage httpRequestMessage)
		{
			RetryPolicy.ExecuteAndCapture(() =>
			{
				NetworkCenter.Current.Execute("status", () =>
				{
					HttpSender.Client.SendAsync(httpRequestMessage).Result.EnsureSuccessStatusCode();
				});
			});
		}

		private static void SetToken(HttpRequestMessage httpRequestMessage)
		{
			httpRequestMessage.Headers.Add("DotnetSpiderToken", Env.HunServiceToken);
		}
	}
}
