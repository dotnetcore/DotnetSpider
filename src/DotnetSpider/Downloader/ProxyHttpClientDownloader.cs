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
			ILogger<ProxyHttpClientDownloader> logger,
			IProxyService proxyService) : base(httpClientFactory, logger)
		{
			proxyService.NotNull(nameof(proxyService));

			_proxyService = proxyService;
		}

		public override string Name => DownloaderNames.ProxyHttpClient;

		protected override async Task<HttpClient> CreateClientAsync(Request request)
		{
			var proxy = await _proxyService.GetAsync(request.Timeout);
			if (proxy == null)
			{
				throw new SpiderException("获取代理失败");
			}

			var httpClient = HttpClientFactory.CreateClient($"{Consts.ProxyPrefix}{proxy}");
			return httpClient;
		}
	}
}
