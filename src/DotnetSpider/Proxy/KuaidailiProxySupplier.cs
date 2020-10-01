using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Proxy
{
	public class KuaidailiProxySupplier : IProxySupplier
	{
		private readonly ProxyOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;

		public KuaidailiProxySupplier(IOptions<ProxyOptions> options, IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
			_options = options.Value;
			_options.ProxySupplierUrl.NotNullOrWhiteSpace(nameof(_options.ProxySupplierUrl));
		}

		public async Task<IEnumerable<Uri>> GetProxiesAsync()
		{
			var client = _httpClientFactory.CreateClient("kuaidaili");
			var text = await client.GetStringAsync(_options.ProxySupplierUrl);
			var proxies = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			return proxies.Select(x => new Uri($"http://{x}"));
		}
	}
}
