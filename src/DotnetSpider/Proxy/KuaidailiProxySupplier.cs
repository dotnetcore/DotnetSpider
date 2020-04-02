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
        private readonly SpiderOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;

        public KuaidailiProxySupplier(IOptions<SpiderOptions> options, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _options.ProxySupplierUri.NotNullOrWhiteSpace(nameof(_options.ProxySupplierUri));
        }

        public async Task<IEnumerable<HttpProxy>> GetProxiesAsync()
        {
            var client = _httpClientFactory.CreateClient("kuaidaili");
            var text = await client.GetStringAsync(_options.ProxySupplierUri);
            var proxies = text.Split(new [] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            return proxies.Select(x => new HttpProxy($"http://{x}"));
        }
    }
}