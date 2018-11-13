using DotnetSpider.Proxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if NETFRAMEWORK
using System.Text;
#endif
[assembly: InternalsVisibleTo("DotnetSpider.Node")]

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// Downloader using <see cref="HttpClient"/>
	/// 非线程安全, 请一个线程一个对象
	/// 只要保证一个线程顺序请求, 如果此下载器Cookie需要更新, 则当前HttpClient必是刚使用完后的
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 纯HTTP下载器
	/// </summary>
	public class HttpClientDownloader : Downloader
	{
		class HttpClientObject : IDisposable
		{
			public DateTime LastUseTime { get; set; }
			public HttpClient Client { get; }
			public HttpClientHandler Handler { get; }

			public HttpClientObject(HttpClientHandler handler, bool allowAutoRedirect)
			{
				Handler = handler;
				Client = allowAutoRedirect
					? new HttpClient(new GlobalRedirectHandler(handler), true)
					: new HttpClient(handler, true);
			}

			public void Dispose()
			{
				Client.Dispose();
			}
		}

		private readonly bool _decodeHtml;
		private readonly int _timeout;
		private static int _getHttpClientCount;

		private static readonly ConcurrentDictionary<string, HttpClientObject> HttpClientObjectPool =
			new ConcurrentDictionary<string, HttpClientObject>();

		private HttpClientObject _clientObject;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="timeout">下载超时时间 Download timeout.</param>
		/// <param name="decodeHtml">下载的内容是否需要HTML解码 Whether need to Html Decode.</param>
		public HttpClientDownloader(int timeout = 8000, bool decodeHtml = false)
		{
			_timeout = timeout;
			_decodeHtml = decodeHtml;
		}

		protected override Response DownloadContent(Request request)
		{
			var response = new Response(request);

			if (IfFileExists(request))
			{
				Logger?.LogInformation($"File {request.Url} already exists.");
				return response;
			}

			var httpRequestMessage = GenerateHttpRequestMessage(request);
			HttpResponseMessage httpResponseMessage = null;
			WebProxy proxy = null;
			try
			{
				if (UseFiddlerProxy)
				{
					if (FiddlerProxy == null)
					{
						throw new DownloaderException("Fiddler proxy is null.");
					}
					else
					{
						proxy = FiddlerProxy;
					}
				}
				else
				{
					if (HttpProxyPool.Instance != null)
					{
						proxy = HttpProxyPool.Instance.GetProxy();
						if (proxy == null)
						{
							throw new DownloaderException("No available proxy.");
						}
					}
					else
					{
						_clientObject = GetHttpClient("DEFAULT", AllowAutoRedirect, null);
					}
				}

				_clientObject = GetHttpClient(proxy == null ? "DEFAULT" : $"{proxy.Address}",
					AllowAutoRedirect, proxy);

				httpResponseMessage =
					NetworkCenter.Current.Execute("downloader", () => Task
						.Run(async () => await _clientObject.Client.SendAsync(httpRequestMessage))
						.GetAwaiter()
						.GetResult());

				response.StatusCode = httpResponseMessage.StatusCode;
				EnsureSuccessStatusCode(response.StatusCode);
				response.TargetUrl = httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri;

				var bytes = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
				if (!ExcludeMediaTypes.Any(t => httpResponseMessage.Content.Headers.ContentType.MediaType.Contains(t)))
				{
					if (!DownloadFiles)
					{
						Logger?.LogWarning($"Ignore {request.Url} because media type is not allowed to download.");
					}
					else
					{
						StorageFile(request, bytes);
					}
				}
				else
				{
					var content = ReadContent(request, bytes,
						httpResponseMessage.Content.Headers.ContentType.CharSet);

					if (_decodeHtml && content is string)
					{
#if NETFRAMEWORK
						content =
 System.Web.HttpUtility.UrlDecode(System.Web.HttpUtility.HtmlDecode(content.ToString()), string.IsNullOrEmpty(request.EncodingName) ? Encoding.UTF8 : Encoding.GetEncoding(request.EncodingName));
#else
						content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content.ToString()));
#endif
					}

					response.Content = content;

					DetectContentType(response, httpResponseMessage.Content.Headers.ContentType.MediaType);
				}
			}
			catch (DownloaderException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new DownloaderException($"Unexpected exception when download request: {request.Url}: {e}.");
			}
			finally
			{
				if (HttpProxyPool.Instance != null && proxy != null)
				{
					HttpProxyPool.Instance.ReturnProxy(proxy,
						httpResponseMessage?.StatusCode ?? HttpStatusCode.ServiceUnavailable);
				}

				try
				{
					httpResponseMessage?.Dispose();
				}
				catch (Exception e)
				{
					throw new BypassedDownloaderException($"Close response {request.Url} failed: {e.Message}");
				}
			}

			return response;
		}

		public override void AddCookie(Cookie cookie)
		{
			base.AddCookie(cookie);

			HttpMessageHandler.CookieContainer.Add(cookie);

			foreach (var kv in HttpClientObjectPool)
			{
				kv.Value.Handler.CookieContainer.Add(cookie);
			}
		}

		/// <summary>
		/// Return same <see cref="HttpClientObject"/> instance when <paramref name="hash"/> is same.
		/// This can ensure some pages have same CookieContainer.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hash">分组的哈希 Hashcode to identify different group.</param>
		/// <param name="allowAutoRedirect">是否自动跳转</param>
		/// <param name="proxy">代理</param>
		/// <returns>HttpClientItem</returns>
		private HttpClientObject GetHttpClient(string hash, bool allowAutoRedirect, IWebProxy proxy)
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

			if (HttpClientObjectPool.ContainsKey(hash))
			{
				HttpClientObjectPool[hash].LastUseTime = DateTime.Now;
				return HttpClientObjectPool[hash];
			}
			else
			{
				var handler = new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
					UseProxy = true,
					UseCookies = true,
					AllowAutoRedirect = false,
					Proxy = proxy,
					CookieContainer = CopyCookieContainer()
				};
				var item = new HttpClientObject(handler, allowAutoRedirect) {LastUseTime = DateTime.Now};
				item.Client.Timeout = new TimeSpan(0, 0, 0, _timeout);
				HttpClientObjectPool.TryAdd(hash, item);
				return item;
			}
		}

		private void CleanupPool()
		{
			List<string> needRemoveEntries = new List<string>();
			var now = DateTime.Now;
			foreach (var pair in HttpClientObjectPool)
			{
				if ((now - pair.Value.LastUseTime).TotalSeconds > 240)
				{
					needRemoveEntries.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveEntries)
			{
				var item = HttpClientObjectPool[key];
				if (HttpClientObjectPool.TryRemove(key, out _))
				{
					item.Dispose();
				}
			}
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Url);

			// Headers 的优先级低于 Request.UserAgent 这种特定设置, 因此先加载所有 Headers, 再使用 Request.UserAgent 覆盖
			foreach (var header in request.Headers)
			{
				httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString());
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

			if (request.Method == HttpMethod.Post)
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
				    request.Headers[xRequestedWithHeader].ToString() == "NULL")
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

			return httpRequestMessage;
		}

		private CookieContainer CopyCookieContainer()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, CookieContainer);
				stream.Seek(0, SeekOrigin.Begin);
				return (CookieContainer) formatter.Deserialize(stream);
			}
		}
	}
}