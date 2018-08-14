#if !NET40
using DotnetSpider.Common;
using DotnetSpider.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
				Client = allowAutoRedirect ? new HttpClient(new GlobalRedirectHandler(handler), true) : new HttpClient(handler, true);
			}

			public void Dispose()
			{
				Client.Dispose();
			}
		}

		private readonly bool _decodeHtml;
		private readonly int _timeout;
		private int _getHttpClientCount;
		private readonly Dictionary<string, HttpClientObject> _pool = new Dictionary<string, HttpClientObject>();
		private HttpClientObject _clientObject;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="timeout">下载超时时间 Download timeout.</param>
		/// <param name="decodeHtml">下载的内容是否需要HTML解码 Whether <see cref="Page.Content"/> need to Html Decode.</param>
		public HttpClientDownloader(int timeout = 8000, bool decodeHtml = false)
		{
			_timeout = timeout;
			_decodeHtml = decodeHtml;
		}

		protected override Response DowloadContent(Request request)
		{
			Response response = new Response();
			response.Request = request;

			if (IfFileExists(request))
			{
				Logger.Information($"File {request.Url} already exists.");
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
							throw new DownloaderException("No avaliable proxy.");
						}
					}
					else
					{
						_clientObject = GetHttpClient("DEFAULT", AllowAutoRedirect, null);
					}
				}

				_clientObject = GetHttpClient(proxy == null ? "DEFAULT" : $"{proxy.Address.ToString()}", AllowAutoRedirect, proxy);

				httpResponseMessage =
						NetworkCenter.Current.Execute("downloader", () => Task.Run(async () =>
						{
							return await _clientObject.Client.SendAsync(httpRequestMessage);
						})
						.GetAwaiter()
						.GetResult());

				response.StatusCode = httpResponseMessage.StatusCode;
				EnsureSuccessStatusCode(response.StatusCode);
				response.TargetUrl = httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri;

				var bytes = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
				if (!request.Site.ExcludeMediaTypes.Any(t => httpResponseMessage.Content.Headers.ContentType.MediaType.Contains(t)))
				{
					if (!request.Site.DownloadFiles)
					{
						Logger.Warning($"Ignore {request.Url} because media type is not allowed to download.");
					}
					else
					{
						StorageFile(request, bytes);
					}
				}
				else
				{
					string content = ReadContent(bytes, httpResponseMessage.Content.Headers.ContentType.CharSet, request.Site);

					if (_decodeHtml)
					{
#if NETFRAMEWORK
						content =
 System.Web.HttpUtility.UrlDecode(System.Web.HttpUtility.HtmlDecode(content), string.IsNullOrEmpty(request.Site.EncodingName) ? Encoding.Default : Encoding.GetEncoding(request.Site.EncodingName));
#else
						content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content));
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
					HttpProxyPool.Instance.ReturnProxy(proxy, httpResponseMessage == null ? HttpStatusCode.ServiceUnavailable : httpResponseMessage.StatusCode);
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
			if (HttpProxyPool.Instance != null)
			{
				HttpMessageHandler.CookieContainer.Add(cookie);
			}
			else
			{
				if ((DateTime.Now - _clientObject.LastUseTime).TotalSeconds <= 240)
				{
					_clientObject.Handler.CookieContainer.Add(cookie);
				}
				else
				{
					Logger.Warning("HttpClient is out of used.");
				}
			}
		}

		/// <summary>
		/// Get a <see cref="HttpClientObject"/> from <see cref="IHttpClientPool"/>.
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

			if (_pool.ContainsKey(hash))
			{
				_pool[hash].LastUseTime = DateTime.Now;
				return _pool[hash];
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
				var item = new HttpClientObject(handler, allowAutoRedirect) { LastUseTime = DateTime.Now };
				_pool.Add(hash, item);
				return item;
			}
		}

		private void CleanupPool()
		{
			List<string> needRemoveEntries = new List<string>();
			var now = DateTime.Now;
			foreach (var pair in _pool)
			{
				if ((now - pair.Value.LastUseTime).TotalSeconds > 240)
				{
					needRemoveEntries.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveEntries)
			{
				var item = _pool[key];
				if (_pool.Remove(key))
				{
					item.Dispose();
				}
			}
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Url);
			var userAgentHeader = "User-Agent";
			httpRequestMessage.Headers.TryAddWithoutValidation(userAgentHeader, request.Site.Headers.ContainsKey(userAgentHeader) ? request.Site.Headers[userAgentHeader] : request.Site.UserAgent);

			if (!string.IsNullOrWhiteSpace(request.Referer))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("Referer", request.Referer);
			}

			if (!string.IsNullOrWhiteSpace(request.Origin))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("Origin", request.Origin);
			}

			if (!string.IsNullOrWhiteSpace(request.Site.Accept))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("Accept", request.Site.Accept);
			}

			var contentTypeHeader = "Content-Type";

			foreach (var header in request.Site.Headers)
			{
				if (header.Key.ToLower() == "cookie")
				{
					continue;
				}
				if (!string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value) && header.Key != contentTypeHeader && header.Key != userAgentHeader)
				{
					httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			if (request.Method == HttpMethod.Post)
			{
				var bytes = CompressContent(request);
				httpRequestMessage.Content = new ByteArrayContent(bytes);

				if (request.Site.Headers.ContainsKey(contentTypeHeader))
				{
					httpRequestMessage.Content.Headers.TryAddWithoutValidation(contentTypeHeader, request.Site.Headers[contentTypeHeader]);
				}

				var xRequestedWithHeader = "X-Requested-With";
				if (request.Site.Headers.ContainsKey(xRequestedWithHeader) && request.Site.Headers[xRequestedWithHeader] == "NULL")
				{
					httpRequestMessage.Content.Headers.Remove(xRequestedWithHeader);
				}
				else
				{
					if (!httpRequestMessage.Content.Headers.Contains(xRequestedWithHeader) && !httpRequestMessage.Headers.Contains(xRequestedWithHeader))
					{
						httpRequestMessage.Content.Headers.TryAddWithoutValidation(xRequestedWithHeader, "XMLHttpRequest");
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
				return (CookieContainer)formatter.Deserialize(stream);
			}
		}
	}
}
#endif
