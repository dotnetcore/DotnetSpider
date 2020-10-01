using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Http;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	public class HttpClientDownloader : IDownloader
	{
		protected IHttpClientFactory HttpClientFactory { get; }
		protected ILogger Logger { get; }

		public HttpClientDownloader(IHttpClientFactory httpClientFactory,
			ILogger<HttpClientDownloader> logger)
		{
			HttpClientFactory = httpClientFactory;
			Logger = logger;
		}

		public async Task<Response> DownloadAsync(Request request)
		{
			HttpResponseMessage httpResponseMessage = null;
			try
			{
				var httpRequestMessage = request.ToHttpRequestMessage();

				var httpClient = await CreateClientAsync(request);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
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
				DisposeSafely(httpResponseMessage);
			}
		}

		protected virtual Task<HttpClient> CreateClientAsync(Request request)
		{
			var httpClient = HttpClientFactory.CreateClient(request.RequestUri.Host);
			return Task.FromResult(httpClient);
		}

		protected virtual Task<Response> HandleAsync(Request request, HttpResponseMessage responseMessage)
		{
			return Task.FromResult((Response)null);
		}

		public virtual string Name => DownloaderNames.HttpClient;

		private void DisposeSafely(IDisposable obj)
		{
			try
			{
				obj?.Dispose();
			}
			catch (Exception e)
			{
				Logger.LogWarning($"Dispose {obj} failed: {e}");
			}
		}
	}
}
