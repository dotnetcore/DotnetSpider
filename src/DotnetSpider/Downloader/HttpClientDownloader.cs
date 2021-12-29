using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Proxy;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	public class HttpClientDownloader : IDownloader
	{
		private readonly IProxyService _proxyService;
		protected IHttpClientFactory HttpClientFactory { get; }
		protected ILogger Logger { get; }
		protected bool UseProxy { get; }

		public HttpClientDownloader(IHttpClientFactory httpClientFactory,
			IProxyService proxyService,
			ILogger<HttpClientDownloader> logger)
		{
			HttpClientFactory = httpClientFactory;
			Logger = logger;
			_proxyService = proxyService;
			UseProxy = !(_proxyService is EmptyProxyService);
		}

		public async Task<Response> DownloadAsync(Request request)
		{
			HttpResponseMessage httpResponseMessage = null;
			HttpRequestMessage httpRequestMessage = null;
			try
			{
				httpRequestMessage = request.ToHttpRequestMessage();

				var httpClient = await CreateClientAsync(request);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				httpResponseMessage = await SendAsync(httpClient, httpRequestMessage);

				stopwatch.Stop();

				var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

				var response = await HandleAsync(request, httpResponseMessage);
				if (response != null)
				{
					response.Version = response.Version == null ? HttpVersion.Version11 : response.Version;
					return response;
				}

				response = await httpResponseMessage.ToResponseAsync();
				response.ElapsedMilliseconds = (int)elapsedMilliseconds;
				response.RequestHash = request.Hash;
				response.Version = httpResponseMessage.Version;

				return response;
			}
			catch (Exception e)
			{
				Logger.LogError($"{request.RequestUri} download failed: {e}");
				return new Response
				{
					RequestHash = request.Hash,
					StatusCode = HttpStatusCode.Gone,
					ReasonPhrase = e.ToString(),
					Version = HttpVersion.Version11
				};
			}
			finally
			{
				ObjectUtilities.DisposeSafely(Logger, httpResponseMessage, httpRequestMessage);
			}
		}

		protected virtual async Task<HttpResponseMessage> SendAsync(HttpClient httpClient,
			HttpRequestMessage httpRequestMessage)
		{
			return await httpClient.SendAsync(httpRequestMessage);
		}

		protected virtual async Task<HttpClient> CreateClientAsync(Request request)
		{
			string name;
			if (UseProxy)
			{
				var proxy = await _proxyService.GetAsync(request.Timeout);
				if (proxy == null)
				{
					throw new SpiderException("获取代理失败");
				}

				name = $"{Const.ProxyPrefix}{proxy}";
			}
			else
			{
				name = request.RequestUri.Host;
			}

			return HttpClientFactory.CreateClient(name);
		}

		protected virtual Task<Response> HandleAsync(Request request, HttpResponseMessage responseMessage)
		{
			return Task.FromResult((Response)null);
		}

		public virtual string Name => UseProxy ? Downloaders.ProxyHttpClient : Downloaders.HttpClient;
	}
}
