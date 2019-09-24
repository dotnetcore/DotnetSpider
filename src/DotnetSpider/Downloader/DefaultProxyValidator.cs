using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Common;

namespace DotnetSpider.Downloader
{
	public class DefaultProxyValidator : IProxyValidator
	{
		private readonly string _targetUrl;

		private readonly ConcurrentDictionary<string, HttpClient> _httpClientDict =
			new ConcurrentDictionary<string, HttpClient>();

		public DefaultProxyValidator(string targetUrl = "http://www.baidu.com")
		{
			_targetUrl = targetUrl;
			if (string.IsNullOrEmpty(_targetUrl))
			{
				throw new SpiderException($"{nameof(targetUrl)} is empty/null");
			}

			if (!Uri.TryCreate(targetUrl, UriKind.RelativeOrAbsolute, out _))
			{
				throw new SpiderException($"{nameof(targetUrl)} is not an uri");
			}
		}

		public async Task<bool> IsAvailable(WebProxy proxy)
		{
			if (proxy == null)
			{
				return false;
			}

			var key = proxy.Address.ToString();
			_httpClientDict.TryAdd(key, new HttpClient(new HttpClientHandler {Proxy = proxy, UseProxy = true}));
			if (_httpClientDict.TryGetValue(key, out var httpClient))
			{
				var msg = new HttpRequestMessage(HttpMethod.Get, _targetUrl);
				var response = await httpClient.SendAsync(msg);
				return response.IsSuccessStatusCode;
			}
			else
			{
				return false;
			}
		}
	}
}
