using DotnetSpider.Core.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DotnetSpider.Core.Proxy
{
	/// <summary>
	/// 实现快代理的提取
	/// </summary>
	public class KuaidailiProxySupplier : IProxySupplier
	{
		/// <summary>
		/// 快代理的提取接口
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">快代理的提取接口</param>
		public KuaidailiProxySupplier(string url)
		{
			Url = url;
		}

		/// <summary>
		/// 取得所有代理
		/// </summary>
		/// <returns>代理</returns>
		public Dictionary<string, Proxy> GetProxies()
		{
			var proxies = new Dictionary<string, Proxy>();
			while (proxies.Count == 0)
			{
				try
				{
					string result = HttpClientPool.HttpClient.GetStringAsync(Url).Result;
					if (!string.IsNullOrWhiteSpace(result))
					{
						if (result.Contains("ERROR(-51)"))
						{
							return proxies;
						}
						foreach (var proxy in result.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
						{
							if (!proxies.ContainsKey(proxy))
							{
								proxies.Add(proxy, new Proxy(new UseSpecifiedUriWebProxy(new Uri($"http://{proxy}"))));
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Logger.Error($"Get new proxies failed: {ex}.");
					Thread.Sleep(5000);
				}
			}
			return proxies;
		}
	}
}
