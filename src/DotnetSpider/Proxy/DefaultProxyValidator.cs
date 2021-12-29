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
		private readonly ProxyOptions _options;
		private readonly ILogger<DefaultProxyValidator> _logger;

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
		}

		public async Task<bool> IsAvailable(Uri proxy)
		{
			if (proxy == null)
			{
				return false;
			}

			var httpClient = _httpClientFactory.CreateClient($"{Const.ProxyPrefix}{proxy}");

			try
			{
				var msg = new HttpRequestMessage(HttpMethod.Head, _options.ProxyTestUrl);
				msg.Headers.TryAddWithoutValidation(Const.ProxyTestUrl, "true");
				var response = await httpClient.SendAsync(msg);
				var isSuccessStatusCode = response.IsSuccessStatusCode;
				return isSuccessStatusCode;
			}
			catch
			{
				_logger.LogWarning($"proxy {proxy} is not available");
				return false;
			}
		}
	}
}
