using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Proxy
{
    public class DefaultProxyValidator : IProxyValidator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SpiderOptions _options;
        private readonly ILogger<DefaultProxyValidator> _logger;

        public DefaultProxyValidator(IOptions<SpiderOptions> options, IHttpClientFactory httpClientFactory,
            ILogger<DefaultProxyValidator> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;

            _options.ProxyTestUrl.NotNullOrWhiteSpace(nameof(_options.ProxyTestUrl));

            if (!Uri.TryCreate(_options.ProxyTestUrl, UriKind.RelativeOrAbsolute, out _))
            {
                throw new ArgumentException($"{nameof(_options.ProxyTestUrl)} is not a valid uri");
            }
        }

        public async Task<bool> IsAvailable(HttpProxy proxy)
        {
            if (proxy == null)
            {
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient($"PROXY_{proxy.Uri}");
            httpClient.Timeout = new TimeSpan(0, 0, 3);
            try
            {
                var msg = new HttpRequestMessage(HttpMethod.Head, _options.ProxyTestUrl);
                var response = await httpClient.SendAsync(msg);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogWarning($"Proxy {proxy.Uri} is not available");
                return false;
            }
        }
    }
}