using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	public class ProxyHttpClientDownloader : HttpClientDownloader
	{
		private readonly IProxyService _proxyService;

		public ProxyHttpClientDownloader(IHttpClientFactory httpClientFactory,
			ILogger<ProxyHttpClientDownloader> logger, IProxyService proxyService) : base(httpClientFactory, logger)
		{
			_proxyService = proxyService;
		}

		public override string Name => DownloaderNames.ProxyHttpClient;

		protected override async Task<HttpClientEntry> CreateAsync(Request request)
		{
			var host = new Uri(request.Url).Host;
			string clientName;
			ProxyEntry proxy = null;
			if (_proxyService != null)
			{
				proxy = await _proxyService.GetAsync(request.Timeout);
				if (proxy == null)
				{
					throw new SpiderException("获取代理失败");
				}

				// todo: uri should contains user/password
				clientName = $"{Consts.ProxyPrefix}{proxy.Uri}";
			}
			else
			{
				clientName = host;
			}

			var httpClient = HttpClientFactory.CreateClient(clientName);
			return new HttpClientEntry(httpClient, proxy);
		}

		protected override void Release(Response response, HttpClientEntry httpClientEntry)
		{
			_proxyService.ReturnAsync(httpClientEntry.Resource,
				response?.StatusCode ?? HttpStatusCode.NotFound);
		}
	}
}
