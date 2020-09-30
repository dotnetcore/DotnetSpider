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
			Response response = null;
			HttpClientEntry httpClientEntry = null;
			HttpResponseMessage httpResponseMessage = null;
			try
			{
				var httpRequest = request.ToHttpRequestMessage();
				httpClientEntry = await CreateAsync(request);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				httpResponseMessage = await httpClientEntry.HttpClient.SendAsync(httpRequest);
				stopwatch.Stop();

				response = await HandleAsync(request, httpResponseMessage);
				if (response != null)
				{
					return response;
				}

				response = await httpResponseMessage.ToResponseAsync();
				response.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
				response.RequestHash = request.Hash;

				return response;
			}
			catch (Exception e)
			{
				Logger.LogError($"{request.RequestUri} download failed: {e}");
				response = new Response
				{
					RequestHash = request.Hash, StatusCode = HttpStatusCode.Gone, ReasonPhrase = e.ToString()
				};
				return response;
			}
			finally
			{
				DisposeSafely(httpResponseMessage);

				Release(response, httpClientEntry);
			}
		}

		protected virtual void Release(Response response, HttpClientEntry httpClientEntry)
		{
		}

		protected virtual Task<Response> HandleAsync(Request request, HttpResponseMessage responseMessage)
		{
			return Task.FromResult((Response)null);
		}

		public virtual string Name => DownloaderNames.HttpClient;

		protected virtual Task<HttpClientEntry> CreateAsync(Request request)
		{
			var host = request.RequestUri.Host;
			var httpClient = HttpClientFactory.CreateClient(host);
			return Task.FromResult(new HttpClientEntry(httpClient, null));
		}

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
