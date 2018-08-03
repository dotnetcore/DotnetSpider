using Serilog;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Text;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Infrastructure
{
	public class HubService
	{
		private static readonly RetryPolicy RetryPolicy = Policy.Handle<Exception>().Retry(5, (ex, count) =>
		{
			Log.Logger.Error($"Submit http log failed [{count}]: {ex}");
		});

		public static void HttpStatus(string status)
		{
			HttpRequestMessage httpRequestMessage = GenerateHttpRequestMessage(status, Env.HubServiceStatusApiUrl);
			Send(httpRequestMessage);
		}


		public static void Send(HttpRequestMessage httpRequestMessage)
		{
			RetryPolicy.ExecuteAndCapture(() =>
			{
				NetworkCenter.Current.Execute("status", () =>
				{
					DotnetSpider.Downloader.Downloader.Default.SendAsync(httpRequestMessage).Result.EnsureSuccessStatusCode();
				});
			});
		}

		public static HttpRequestMessage GenerateHttpRequestMessage(string data, string api)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, api);
			SetToken(httpRequestMessage);
			httpRequestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
			return httpRequestMessage;
		}

		private static void SetToken(HttpRequestMessage httpRequestMessage)
		{
			httpRequestMessage.Headers.Add("DotnetSpiderToken", Env.HubServiceToken);
		}
	}
}
