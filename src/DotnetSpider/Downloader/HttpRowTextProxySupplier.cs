using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    public class HttpRowTextProxySupplier : IProxySupplier
    {
        private readonly string _url;

        private readonly HttpClient _client = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseProxy = true,
            UseCookies = true,
            MaxAutomaticRedirections = 10
        });

        public HttpRowTextProxySupplier(string url)
        {
            _url = url;
        }

        public async Task<Dictionary<string, Proxy>> GetProxies()
        {
            var rows = await _client.GetStringAsync(_url);
            var proxies = rows.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var dict = new Dictionary<string, Proxy>();
            foreach (var proxy in proxies)
            {
                if (!dict.ContainsKey(proxy))
                {
                    dict.Add(proxy, new Proxy(new WebProxy(proxy, false)));
                }
            }

            return dict;
        }
    }
}