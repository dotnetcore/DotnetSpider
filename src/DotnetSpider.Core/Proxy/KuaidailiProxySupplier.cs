using System;
using System.Collections.Generic;
using System.Net.Http;
namespace DotnetSpider.Core.Proxy
{
	public class KuaidailiProxySupplier : IProxySupplier
	{
		public string Url { get; }
		private readonly HttpClient _client = new HttpClient();

		public KuaidailiProxySupplier(string url)
		{
			Url = url;
		}

		public Dictionary<string, Proxy> GetProxies()
		{
			var list = new Dictionary<string, Proxy>();
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
			return list;
		}
	}
}
