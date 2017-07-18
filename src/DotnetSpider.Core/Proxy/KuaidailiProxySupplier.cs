using DotnetSpider.Core.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace DotnetSpider.Core.Proxy
{
	public class KuaidailiProxySupplier : IProxySupplier
	{
		private readonly static ILogger Logger = LogCenter.GetLogger();
		public string Url { get; }
		private readonly HttpClient _client = new HttpClient();

		public KuaidailiProxySupplier(string url)
		{
			Url = url;
		}

		public Dictionary<string, Proxy> GetProxies()
		{
			var list = new Dictionary<string, Proxy>();
			while (list.Count == 0)
			{
				try
				{
					string result = _client.GetStringAsync(Url).Result;
					if (!string.IsNullOrEmpty(result))
					{
						if (result.Contains("ERROR(-51)"))
						{
							return list;
						}
						foreach (var proxy in result.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
						{
							if (!list.ContainsKey(proxy))
							{
								list.Add(proxy, new Proxy(new UseSpecifiedUriWebProxy(new Uri($"http://{proxy}"))));
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.MyLog("Get new proxies failed.", LogLevel.Error, ex);
					Thread.Sleep(5000);
				}
			}
			return list;
		}
	}
}
