using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using LZ4;
using Microsoft.Extensions.Logging;
using Cookie = DotnetSpider.Common.Cookie;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 基于 HttpClient 实现的下载器
	/// </summary>
	public class HttpClientDownloader : DownloaderBase
	{
		private int _getHttpClientCount;
		private readonly CookieContainer _cookieContainer = new CookieContainer();

		private readonly ConcurrentDictionary<string, HttpClientEntry> _httpClients =
			new ConcurrentDictionary<string, HttpClientEntry>();

		public int RetryTime { get; set; } = 3;

		/// <summary>
		/// 是否自动跳转
		/// </summary>
		public bool AllowAutoRedirect { get; set; } = true;

		public bool UseProxy { get; set; }

		public bool UseCookies { get; set; } = true;

		public bool DecodeHtml { get; set; }

		public int Timeout { get; set; } = 8000;

		public override void AddCookies(params Cookie[] cookies)
		{
			if (cookies != null && cookies.Length > 0)
			{
				foreach (var cookie in cookies)
				{
					_cookieContainer.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
				}
			}
		}

		protected override async Task<Response> ImplDownloadAsync(Request request)
		{
			var response = new Response
			{
				Request = request
			};

			for (int i = 0; i < RetryTime; ++i)
			{
				HttpResponseMessage httpResponseMessage = null;
				WebProxy proxy = null;
				try
				{
					var httpRequestMessage = GenerateHttpRequestMessage(request);

					if (UseProxy)
					{
						if (HttpProxyPool == null)
						{
							response.Exception = "未正确配置代理池";
							response.Success = false;
							Logger?.LogError(
								$"任务 {request.OwnerId} 下载 {request.Url} 失败 [{i}]: {response.Exception}");
							return response;
						}
						else
						{
							proxy = HttpProxyPool.GetProxy();
							if (proxy == null)
							{
								response.Exception = "没有可用的代理";
								response.Success = false;
								Logger?.LogError(
									$"任务 {request.OwnerId} 下载 {request.Url} 失败 [{i}]: {response.Exception}");
								return response;
							}
						}
					}

					var httpClientEntry = GetHttpClientEntry(proxy == null ? "DEFAULT" : $"{proxy.Address}", proxy);


					httpResponseMessage = Framework.NetworkCenter == null
						? await httpClientEntry.HttpClient.SendAsync(httpRequestMessage)
						: await Framework.NetworkCenter.Execute(async () =>
							await httpClientEntry.HttpClient.SendAsync(httpRequestMessage));

					httpResponseMessage.EnsureSuccessStatusCode();
					response.TargetUrl = httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri;
					var bytes = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
					if (!ExcludeMediaTypes.Any(t =>
						httpResponseMessage.Content.Headers.ContentType.MediaType.Contains(t)))
					{
						if (!DownloadFile)
						{
							StorageFile(request, bytes);
						}
					}
					else
					{
						var content = ReadContent(request, bytes,
							httpResponseMessage.Content.Headers.ContentType.CharSet);

						if (DecodeHtml)
						{
#if NETFRAMEWORK
                            content = System.Web.HttpUtility.UrlDecode(
                                System.Web.HttpUtility.HtmlDecode(content),
                                string.IsNullOrEmpty(request.Encoding)
                                    ? Encoding.UTF8
                                    : Encoding.GetEncoding(request.Encoding));
#else
							content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content));
#endif
						}

						response.RawText = content;
					}

					if (!string.IsNullOrWhiteSpace(request.ChangeIpPattern) &&
					    Regex.IsMatch(response.RawText, request.ChangeIpPattern))
					{
						if (UseProxy)
						{
							response.TargetUrl = null;
							response.RawText = null;
							response.Success = false;
							// 把代理设置为空，影响 final 代码块里不作归还操作，等于删除此代理
							proxy = null;
						}
						else
						{
							// 不支持切换 IP
							if (Framework.NetworkCenter == null ||
							    !Framework.NetworkCenter.SupportAdsl)
							{
								response.Success = false;
								response.Exception = "IP Banded";
								Logger?.LogError(
									$"任务 {request.OwnerId} 下载 {request.Url} 失败 [{i}]: {response.Exception}");
								return response;
							}
							else
							{
								Framework.NetworkCenter.Redial();
							}
						}
					}
					else
					{
						response.Success = true;
						Logger?.LogInformation(
							$"任务 {request.OwnerId} 下载 {request.Url} 成功");
						return response;
					}
				}
				catch (Exception e)
				{
					response.Exception = e.Message;
					response.Success = false;
					Logger?.LogError($"任务 {request.OwnerId} 下载 {request.Url} 失败 [{i}]: {e}");
				}
				finally
				{
					if (HttpProxyPool != null && proxy != null)
					{
						HttpProxyPool.ReturnProxy(proxy,
							httpResponseMessage?.StatusCode ?? HttpStatusCode.ServiceUnavailable);
					}

					try
					{
						httpResponseMessage?.Dispose();
					}
					catch (Exception e)
					{
						Logger?.LogWarning($"任务 {request.OwnerId} 释放 {request.Url} 失败 [{i}]: {e}");
					}
				}
			}

			return response;
		}

		protected virtual string ReadContent(Request request, byte[] contentBytes, string characterSet)
		{
			if (string.IsNullOrEmpty(request.Encoding))
			{
				Encoding htmlCharset = EncodingHelper.GetEncoding(characterSet, contentBytes);
				return htmlCharset.GetString(contentBytes, 0, contentBytes.Length);
			}

			return Encoding.GetEncoding(request.Encoding).GetString(contentBytes, 0, contentBytes.Length);
		}

		protected virtual byte[] CompressContent(Request request)
		{
			var encoding = string.IsNullOrEmpty(request.Encoding)
				? Encoding.UTF8
				: Encoding.GetEncoding(request.Encoding);
			var bytes = encoding.GetBytes(request.Body);

			switch (request.Compression)
			{
				case Compression.Lz4:
				{
					bytes = LZ4Codec.Wrap(bytes);
					break;
				}

				case Compression.None:
				{
					break;
				}

				default:
				{
					throw new NotImplementedException(request.Compression.ToString());
				}
			}

			return bytes;
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request)
		{
			HttpRequestMessage httpRequestMessage =
				new HttpRequestMessage(
					string.IsNullOrWhiteSpace(request.Method)
						? HttpMethod.Get
						: new HttpMethod(request.Method.ToUpper()),
					request.Url);

			// Headers 的优先级低于 Request.UserAgent 这种特定设置, 因此先加载所有 Headers, 再使用 Request.UserAgent 覆盖
			foreach (var header in request.Headers)
			{
				httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (!string.IsNullOrWhiteSpace(request.UserAgent))
			{
				var header = "User-Agent";
				httpRequestMessage.Headers.Remove(header);
				httpRequestMessage.Headers.TryAddWithoutValidation(header, request.UserAgent);
			}

			if (!string.IsNullOrWhiteSpace(request.Referer))
			{
				var header = "Referer";
				httpRequestMessage.Headers.Remove(header);
				httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Referer);
			}

			if (!string.IsNullOrWhiteSpace(request.Origin))
			{
				var header = "Origin";
				httpRequestMessage.Headers.Remove(header);
				httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Origin);
			}

			if (!string.IsNullOrWhiteSpace(request.Accept))
			{
				var header = "Accept";
				httpRequestMessage.Headers.Remove(header);
				httpRequestMessage.Headers.TryAddWithoutValidation(header, request.Accept);
			}

			if (request.Method?.ToUpper() == "POST")
			{
				var bytes = CompressContent(request);
				httpRequestMessage.Content = new ByteArrayContent(bytes);

				if (!string.IsNullOrWhiteSpace(request.ContentType))
				{
					var header = "Content-Type";
					httpRequestMessage.Content.Headers.Remove(header);
					httpRequestMessage.Content.Headers.TryAddWithoutValidation(header, request.ContentType);
				}

				var xRequestedWithHeader = "X-Requested-With";
				if (request.Headers.ContainsKey(xRequestedWithHeader) &&
				    request.Headers[xRequestedWithHeader] == "NULL")
				{
					httpRequestMessage.Content.Headers.Remove(xRequestedWithHeader);
				}
				else
				{
					if (!httpRequestMessage.Content.Headers.Contains(xRequestedWithHeader) &&
					    !httpRequestMessage.Headers.Contains(xRequestedWithHeader))
					{
						httpRequestMessage.Content.Headers.TryAddWithoutValidation(xRequestedWithHeader,
							"XMLHttpRequest");
					}
				}
			}

			if (!string.IsNullOrEmpty(request.Cookie))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("Cookie", request.Cookie);
			}

			return httpRequestMessage;
		}

		/// <summary>
		/// Return same <see cref="HttpClientEntry"/> instance when <paramref name="hash"/> is same.
		/// This can ensure some pages have same CookieContainer.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hash">分组的哈希 Hashcode to identify different group.</param>
		/// <param name="proxy">代理</param>
		/// <returns>HttpClientItem</returns>
		private HttpClientEntry GetHttpClientEntry(string hash, IWebProxy proxy)
		{
			if (string.IsNullOrWhiteSpace(hash))
			{
				hash = string.Empty;
			}

			Interlocked.Increment(ref _getHttpClientCount);

			if (_getHttpClientCount % 100 == 0)
			{
				CleanupPool();
			}

			if (_httpClients.ContainsKey(hash))
			{
				_httpClients[hash].LastUseTime = DateTime.Now;
				return _httpClients[hash];
			}

			var handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = UseProxy,
				UseCookies = UseCookies,
				AllowAutoRedirect = false,
				Proxy = proxy,
				CookieContainer = _cookieContainer
			};
			var item = new HttpClientEntry(handler, AllowAutoRedirect) {LastUseTime = DateTime.Now};
			item.HttpClient.Timeout = new TimeSpan(0, 0, 0, Timeout);
			_httpClients.TryAdd(hash, item);
			return item;
		}

		private void CleanupPool()
		{
			List<string> needRemoveEntries = new List<string>();
			var now = DateTime.Now;
			foreach (var pair in _httpClients)
			{
				if ((now - pair.Value.LastUseTime).TotalSeconds > 240)
				{
					needRemoveEntries.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveEntries)
			{
				var item = _httpClients[key];
				if (_httpClients.TryRemove(key, out _))
				{
					item.Dispose();
				}
			}
		}

		private class HttpClientEntry : IDisposable
		{
			public DateTime LastUseTime { get; set; }

			public HttpClient HttpClient { get; }

			public HttpClientEntry(HttpClientHandler handler, bool allowAutoRedirect)
			{
				var handler1 = handler;
				HttpClient = allowAutoRedirect
					? new HttpClient(new GlobalRedirectHandler(handler1), true)
					: new HttpClient(handler1, true);
			}

			public void Dispose()
			{
				HttpClient.Dispose();
			}
		}
	}
}