using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Proxy;
using Microsoft.Extensions.Logging;
using ByteArrayContent = DotnetSpider.Http.ByteArrayContent;

namespace DotnetSpider.Downloader
{
	public class PPPoEHttpClientDownloader : HttpClientDownloader
	{
		private readonly PPPoEService _pppoeService;

		public PPPoEHttpClientDownloader(IHttpClientFactory httpClientFactory,
			ILogger<PPPoEHttpClientDownloader> logger,
			IProxyService proxyService,
			PPPoEService pppoeService) : base(httpClientFactory, proxyService, logger)
		{
			_pppoeService = pppoeService;
			if (!_pppoeService.IsActive)
			{
				throw new SpiderException("PPoE 配置不正确");
			}
		}

		public override string Name => Downloaders.PPPoEHttpClient;

		protected override async Task<Response> HandleAsync(Request request, HttpResponseMessage responseMessage)
		{
			Response response = null;

			// todo: 要考虑并发过程中切换拨号的问题
			var text = await responseMessage.Content.ReadAsStringAsync();
			var validResult = await _pppoeService.DetectAsync(request, text);
			if (!string.IsNullOrWhiteSpace(validResult))
			{
				Logger.LogError(
					$"{request.RequestUri} download failed, because content contains {validResult}");
				response = new Response
				{
					RequestHash = request.Hash,
					StatusCode = HttpStatusCode.BadGateway,
					Content = new ByteArrayContent(
						Encoding.UTF8.GetBytes($"Redial agent because content contains {validResult}"))
				};
			}

			return response;
		}
	}
}
