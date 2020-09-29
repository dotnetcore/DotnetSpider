using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Http;
using Microsoft.Extensions.Logging;
using ByteArrayContent = DotnetSpider.Http.ByteArrayContent;
using StringContent = DotnetSpider.Http.StringContent;

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
				var httpRequest = GenerateHttpRequestMessage(request);
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

				response = new Response
				{
					ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
					StatusCode = httpResponseMessage.StatusCode
				};
				foreach (var header in httpResponseMessage.Headers)
				{
					response.Headers.Add(header.Key, new HashSet<string>(header.Value));
				}

				response.RequestHash = request.Hash;
				response.Content = new ResponseContent
				{
					Data = await httpResponseMessage.Content.ReadAsByteArrayAsync()
				};
				foreach (var header in httpResponseMessage.Content.Headers)
				{
					response.Content.Headers.Add(header.Key, new HashSet<string>(header.Value));
				}

				return response;
			}
			catch (Exception e)
			{
				Logger.LogError($"{request.Url} download failed: {e}");
				response = new Response
				{
					RequestHash = request.Hash,
					StatusCode = HttpStatusCode.BadGateway,
					Content = new ResponseContent {Data = Encoding.UTF8.GetBytes(e.ToString())}
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
			var host = new Uri(request.Url).Host;
			var httpClient = HttpClientFactory.CreateClient(host);
			return Task.FromResult(new HttpClientEntry(httpClient, null));
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request)
		{
			var httpRequestMessage =
				new HttpRequestMessage(
					string.IsNullOrWhiteSpace(request.Method)
						? HttpMethod.Get
						: new HttpMethod(request.Method.ToUpper()),
					request.Url);

			foreach (var header in request.Headers)
			{
				httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (string.IsNullOrWhiteSpace(request.UserAgent))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("User-Agent",
					"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36 Edg/80.0.361.69");
			}

			if (request.Method.ToUpper() == "POST")
			{
				var content = request.GetContentObject();
				if (content != null)
				{
					if (content is StringContent stringContent)
					{
						httpRequestMessage.Content = new System.Net.Http.StringContent(
							stringContent.Content,
							Encoding.GetEncoding(stringContent.EncodingName), stringContent.MediaType);
					}
					else if (content is ByteArrayContent byteArrayContent && byteArrayContent.Bytes != null)
					{
						httpRequestMessage.Content = new System.Net.Http.ByteArrayContent(byteArrayContent.Bytes);
					}

					foreach (var header in content.Headers)
					{
						httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
					}
				}
			}

			return httpRequestMessage;
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
