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

namespace DotnetSpider.Agent
{
	public class HttpClientDownloader : IDownloader
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger _logger;
		private readonly PPPoEService _pppoeService;

		public HttpClientDownloader(IHttpClientFactory httpClientFactory,
			PPPoEService pppoeService,
			ILogger<HttpClientDownloader> logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_pppoeService = pppoeService;
		}

		public async Task<Response> DownloadAsync(Request request)
		{
			try
			{
				var clientName = string.IsNullOrWhiteSpace(request.Proxy)
					? request.RequestUri.Host
					: $"{Consts.ProxyPrefix}{request.Proxy}";

				var httpClient = _httpClientFactory.CreateClient(clientName);
				var httpRequest = GenerateHttpRequestMessage(request);

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var httpResponseMessage = await httpClient.SendAsync(httpRequest);
				stopwatch.Stop();
				var matchValue = await _pppoeService.DetectAsync(request, httpResponseMessage);
				if (!string.IsNullOrWhiteSpace(matchValue))
				{
					_logger.LogError(
						$"{request.RequestUri} download failed, because content contains {matchValue}");
					return new Response
					{
						RequestHash = request.Hash,
						StatusCode = HttpStatusCode.BadGateway,
						Content = new ResponseContent
						{
							Data = Encoding.UTF8.GetBytes($"Redial agent because content contains {matchValue}")
						}
					};
				}
				else
				{
					var response = new Response
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
			}
			catch (Exception e)
			{
				_logger.LogError($"{request.RequestUri} download failed: {e}");
				return new Response
				{
					RequestHash = request.Hash,
					StatusCode = HttpStatusCode.BadGateway,
					Content = new ResponseContent {Data = Encoding.UTF8.GetBytes(e.ToString())}
				};
			}
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request)
		{
			var httpRequestMessage =
				new HttpRequestMessage(
					string.IsNullOrWhiteSpace(request.Method)
						? HttpMethod.Get
						: new HttpMethod(request.Method.ToUpper()),
					request.RequestUri);

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
	}
}
