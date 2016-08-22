using System;
using System.Collections.Generic;
using System.Net.Http;
namespace DotnetSpider.Core.Proxy
{
	public class KuaidailiProxySupplier : IProxySupplier
	{
		public string Url { get; } =
			"http://dev.kuaidaili.com/api/getproxy/?orderid=917184806038194&num=999&b_pcchrome=1&b_pcie=1&b_pcff=1&protocol=1&method=2&an_tr=1&an_an=1&an_ha=1&sep=1"
			;
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
