using System;
using System.Collections.Concurrent;
using System.Net;
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
		private readonly ProxyOptions _options;
		private readonly ILogger<DefaultProxyValidator> _logger;
		private readonly ConcurrentDictionary<Uri, HttpClient> _cahche;

		public DefaultProxyValidator(IOptions<ProxyOptions> options,
			IHttpClientFactory httpClientFactory,
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

			_cahche = new ConcurrentDictionary<Uri, HttpClient>();
		}

		public async Task<bool> IsAvailable(Uri proxy)
		{
			if (proxy == null)
			{
				return false;
			}

			var httpClient = _cahche.GetOrAdd(proxy, uri =>
			{
				var handler = new HttpClientHandler
				{
					UseCookies = true,
					UseProxy = true,
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					Proxy = new WebProxy(proxy)
				};
				var client = new HttpClient(handler) {Timeout = new TimeSpan(0, 0, 5)};
				return client;
			});

			try
			{
				var msg = new HttpRequestMessage(HttpMethod.Head, _options.ProxyTestUrl);
				var response = await httpClient.SendAsync(msg);
				var isSuccessStatusCode = response.IsSuccessStatusCode;
				return isSuccessStatusCode;
			}
			catch
			{
				_logger.LogWarning($"Proxy {proxy} is not available");
				return false;
			}
		}
	}
}
