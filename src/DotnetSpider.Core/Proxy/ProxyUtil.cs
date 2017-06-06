using DotnetSpider.Core.Infrastructure;
using System;
using System.Net.Http;

namespace DotnetSpider.Core.Proxy
{
	public class ProxyUtil
	{
		public static bool ValidateProxy(string ip, int port)
		{
			bool isReachable = false;

			string http = $"http://{ip}:{port}";
			try
			{
				HttpClient client = new HttpClient(new HttpClientHandler
				{
					AllowAutoRedirect = true,
					UseProxy = true,
					Proxy = new UseSpecifiedUriWebProxy(new Uri(http))
				})
				{
					Timeout = new TimeSpan(0, 0, 0, 5)
				};
				var result = client.GetStringAsync("http://www.baidu.com").Result;
				if (result.Contains("百度"))
				{
					isReachable = true;
				}
			}
			catch (Exception e)
			{
				LogCenter.Log(null, "FAILRE - CAN not connect! Proxy: " + http, LogLevel.Error, e);
			}

			return isReachable;
		}
	}
}