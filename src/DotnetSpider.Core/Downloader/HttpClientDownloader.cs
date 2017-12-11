using System;
using System.Collections.Generic;
using System.IO;
#if !NET_CORE
using System.Web;
#endif
using System.Text;
using System.Net.Http;
using System.Net;
using DotnetSpider.Core.Infrastructure;
using NLog;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// The http downloader based on HttpClient.
	/// </summary>
	public class HttpClientDownloader : BaseDownloader
	{
		private static readonly HashSet<string> MediaTypes = new HashSet<string>
		{
			"text/html",
			"text/plain",
			"text/richtext",
			"text/xml",
			"text/XML",
			"text/json",
			"text/javascript",
			"application/soap+xml",
			"application/xml",
			"application/json",
			"application/x-javascript",
			"application/javascript",
			"application/x-www-form-urlencoded"
		};
		private readonly HttpClientPool _httpClientPool = new HttpClientPool();
		private readonly HttpClient _httpClient;

		private bool _decodeHtml;

		public HttpClientDownloader()
		{
			_httpClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = false
			});
		}

		public HttpClientDownloader(bool decodeHtml = false, int timeout = 5) : this()
		{
			_httpClient.Timeout = new TimeSpan(0, 0, timeout);
			_decodeHtml = decodeHtml;
		}

		public static void AddMediaTypes(string type)
		{
			MediaTypes.Add(type);
		}

		protected override Page DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;

			HttpResponseMessage response = null;
			var proxy = site.GetHttpProxy();
			request.Proxy = proxy;

			try
			{
				var httpMessage = GenerateHttpRequestMessage(request, site);

				HttpClient httpClient = null == spider.Site.HttpProxyPool ? _httpClient : _httpClientPool.GetHttpClient(proxy);

				response = NetworkCenter.Current.Execute("http", () => httpClient.SendAsync(httpMessage).Result);

				request.StatusCode = response.StatusCode;
				response.EnsureSuccessStatusCode();

				if (!site.AcceptStatCode.Contains(response.StatusCode))
				{
					throw new DownloadException($"Download {request.Url} failed. Code {response.StatusCode}");
				}
				Page page;

				if (response.Content.Headers.ContentType != null && !MediaTypes.Contains(response.Content.Headers.ContentType.MediaType))
				{
					if (!site.DownloadFiles)
					{
						Logger.AllLog(spider.Identity, $"Miss request: {request.Url} because media type is not text.", LogLevel.Error);
						return new Page(request, null) { Skip = true };
					}
					else
					{
						page = SaveFile(request, response, spider);
					}
				}
				else
				{
					page = HandleResponse(request, response, site);

					if (string.IsNullOrEmpty(page.Content))
					{
						Logger.AllLog(spider.Identity, $"Content is empty: {request.Url}.", LogLevel.Warn);
					}
				}

				page.TargetUrl = response.RequestMessage.RequestUri.AbsoluteUri;

				return page;
			}
			catch (DownloadException de)
			{
				Page page = site.CycleRetryTimes > 0 ? Spider.AddToCycleRetry(request, site) : new Page(request, null);

				if (page != null)
				{
					page.Exception = de;
				}
				Logger.AllLog(spider.Identity, $"Download {request.Url} failed: {de.Message}", LogLevel.Warn);

				return page;
			}
			catch (HttpRequestException he)
			{
				Page page = site.CycleRetryTimes > 0 ? Spider.AddToCycleRetry(request, site) : new Page(request, null);
				if (page != null)
				{
					page.Exception = he;
				}

				Logger.AllLog(spider.Identity, $"Download {request.Url} failed: {he.Message}.", LogLevel.Warn);
				return page;
			}
			catch (Exception e)
			{
				Page page = new Page(request, null)
				{
					Exception = e,
					Skip = true
				};

				Logger.AllLog(spider.Identity, $"Download {request.Url} failed: {e.Message}.", LogLevel.Error, e);
				return page;
			}
			finally
			{
				// 先Close Response, 避免前面语句异常导致没有关闭.
				try
				{
					//ensure the connection is released back to pool
					//check:
					//EntityUtils.consume(httpResponse.getEntity());
					response?.Dispose();
				}
				catch (Exception e)
				{
					Logger.AllLog(spider.Identity, "Close response fail.", LogLevel.Error, e);
				}
			}
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request, Site site)
		{
			if (site == null) return null;
			if (site.Headers == null)
			{
				site.Headers = new Dictionary<string, string>();
			}

			HttpRequestMessage httpWebRequest = CreateRequestMessage(request);

			httpWebRequest.Headers.Add("User-Agent", site.Headers.ContainsKey("User-Agent") ? site.Headers["User-Agent"] : site.UserAgent);

			if (!string.IsNullOrEmpty(request.Referer))
			{
				httpWebRequest.Headers.Add("Referer", request.Referer);
			}

			if (!string.IsNullOrEmpty(request.Origin))
			{
				httpWebRequest.Headers.Add("Origin", request.Origin);
			}

			if (!string.IsNullOrEmpty(site.Accept))
			{
				httpWebRequest.Headers.Add("Accept", site.Accept);
			}

			foreach (var header in site.Headers)
			{
				if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value) && header.Key != "Content-Type" && header.Key != "User-Agent")
				{
					httpWebRequest.Headers.Add(header.Key, header.Value);
				}
			}

			httpWebRequest.Headers.Add("Cookie", site.Cookies?.ToString());

			if (httpWebRequest.Method == HttpMethod.Post)
			{
				var data = string.IsNullOrEmpty(site.EncodingName) ? Encoding.UTF8.GetBytes(request.PostBody) : site.Encoding.GetBytes(request.PostBody);
				httpWebRequest.Content = new StreamContent(new MemoryStream(data));

				if (site.Headers.ContainsKey("Content-Type"))
				{
					httpWebRequest.Content.Headers.Add("Content-Type", site.Headers["Content-Type"]);
				}

				if (site.Headers.ContainsKey("X-Requested-With") && site.Headers["X-Requested-With"] == "NULL")
				{
					httpWebRequest.Content.Headers.Remove("X-Requested-With");
				}
				else
				{
					if (!httpWebRequest.Content.Headers.Contains("X-Requested-With") && !httpWebRequest.Headers.Contains("X-Requested-With"))
					{
						httpWebRequest.Content.Headers.Add("X-Requested-With", "XMLHttpRequest");
					}
				}
			}
			return httpWebRequest;
		}

		private HttpRequestMessage CreateRequestMessage(Request request)
		{
			switch (request.Method.Method)
			{
				case "GET":
					{
						return new HttpRequestMessage(HttpMethod.Get, request.Url);
					}
				case "POST":
					{
						return new HttpRequestMessage(HttpMethod.Post, request.Url);
					}
				case "HEAD":
					{
						return new HttpRequestMessage(HttpMethod.Head, request.Url);
					}
				case "PUT":
					{
						return new HttpRequestMessage(HttpMethod.Put, request.Url);
					}
				case "DELETE":
					{
						return new HttpRequestMessage(HttpMethod.Delete, request.Url);
					}
				case "TRACE":
					{
						return new HttpRequestMessage(HttpMethod.Trace, request.Url);
					}
				default:
					{
						throw new ArgumentException($"Illegal HTTP Method: {request.Method}.");
					}
			}
		}

		private Page HandleResponse(Request request, HttpResponseMessage response, Site site)
		{
			string content = ReadContent(site, response);

			if (_decodeHtml)
			{
#if !NET_CORE
				content = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(content), string.IsNullOrEmpty(site.EncodingName) ? Encoding.Default : site.Encoding);
#else
				content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content));
#endif
			}

			Page page = new Page(request, site.RemoveOutboundLinks ? site.Domains : null)
			{
				Content = content
			};

			//foreach (var header in response.Headers)
			//{
			//	page.Request.PutExtra(header.Key, header.Value);
			//}

			return page;
		}

		private string ReadContent(Site site, HttpResponseMessage response)
		{
			byte[] contentBytes = response.Content.ReadAsByteArrayAsync().Result;
			contentBytes = PreventCutOff(contentBytes);
			if (string.IsNullOrEmpty(site.EncodingName))
			{
				var charSet = response.Content.Headers.ContentType?.CharSet;
				Encoding htmlCharset = EncodingExtensions.GetEncoding(charSet, contentBytes);
				return htmlCharset.GetString(contentBytes, 0, contentBytes.Length);
			}
			else
			{
				return site.Encoding.GetString(contentBytes, 0, contentBytes.Length);
			}
		}

		private byte[] PreventCutOff(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == 0x00)
				{
					bytes[i] = 32;
				}
			}
			return bytes;
		}
	}
}
