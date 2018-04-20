using System;
using System.Collections.Generic;
using System.IO;
#if NET45
using System.Web;
#endif
using System.Text;
using System.Net.Http;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using System.Runtime.CompilerServices;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Downloader using <see cref="HttpClient"/>
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 纯HTTP下载器
	/// </summary>
	public class HttpClientDownloader : BaseDownloader
	{
		/// <summary>
		/// What mediatype should not be treated as file to download.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 定义哪些类型的内容不需要当成文件下载
		/// </summary>
		public static HashSet<string> ExcludeMediaTypes = new HashSet<string>
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

		private readonly string _downloadFolder;
		private readonly bool _decodeHtml;
		private readonly double _timeout = 8000;

		/// <summary>
		/// A <see cref="HttpClient"/> pool
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// HttpClient池
		/// </summary>
		public static IHttpClientPool HttpClientPool = new HttpClientPool();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		public HttpClientDownloader()
		{
			_downloadFolder = Path.Combine(Env.BaseDirectory, "download");
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="timeout">下载超时时间 Download timeout.</param>
		/// <param name="decodeHtml">下载的内容是否需要HTML解码 Whether <see cref="Page.Content"/> need to Html Decode.</param>
		public HttpClientDownloader(int timeout = 8000, bool decodeHtml = false) : this()
		{
			_timeout = timeout;
			_decodeHtml = decodeHtml;
		}

		/// <summary>
		/// Add cookies to download clients: HttpClient, WebDriver etc...
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookie 到下载客户端: HttpClient, WebDriver etc...
		/// </summary>
		/// <param name="cookie">Cookie</param>
		protected override void AddCookieToDownloadClient(Cookie cookie)
		{
			HttpClientPool.AddCookie(cookie);
		}

		/// <summary>
		/// Http download implemention
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// HTTP下载的实现
		/// </summary>
		/// <param name="request">请求信息 <see cref="Request"/></param>
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <returns>页面数据 <see cref="Page"/></returns>
		protected override Page DowloadContent(Request request, ISpider spider)
		{
			HttpResponseMessage response = null;
			try
			{
				var httpMessage = GenerateHttpRequestMessage(request, spider.Site);

				HttpClientElement httpClientItem;
				if (spider.Site.HttpProxyPool == null)
				{
					// Request可以设置不同的DownloaderGroup来使用不同的HttpClient
					httpClientItem = HttpClientPool.GetHttpClient(spider, this, CookieContainer, request.DownloaderGroup, CookieInjector);
				}
				else
				{
					// TODO: 代理模式下: request.DownloaderGroup 再考虑
					var proxy = spider.Site.HttpProxyPool.GetProxy();
					request.Proxy = proxy;
					httpClientItem = HttpClientPool.GetHttpClient(spider, this, CookieContainer, proxy?.GetHashCode(), CookieInjector);
					httpClientItem.Handler.Proxy = httpClientItem.Handler.Proxy ?? proxy;
				}
				if (!Equals(httpClientItem.Client.Timeout.TotalSeconds, _timeout))
				{
					httpClientItem.Client.Timeout = new TimeSpan(0, 0, (int)_timeout);
				}

				response = NetworkCenter.Current.Execute("http", () => httpClientItem.Client.SendAsync(httpMessage).Result);
				request.StatusCode = response.StatusCode;
				response.EnsureSuccessStatusCode();

				Page page;

				if (response.Content.Headers.ContentType != null && !ExcludeMediaTypes.Contains(response.Content.Headers.ContentType.MediaType))
				{
					if (!spider.Site.DownloadFiles)
					{
						Logger.Log(spider.Identity, $"Ignore: {request.Url} because media type is not allowed to download.", Level.Warn);
						return new Page(request) { Skip = true };
					}
					else
					{
						page = SaveFile(request, response, spider);
					}
				}
				else
				{
					page = HandleResponse(request, response, spider.Site);

					if (string.IsNullOrWhiteSpace(page.Content))
					{
						Logger.Log(spider.Identity, $"Content is empty: {request.Url}.", Level.Warn);
					}
				}

				page.TargetUrl = response.RequestMessage.RequestUri.AbsoluteUri;

				return page;
			}
			catch (Exception e)
			{
				return CreateRetryPage(e, request, spider);
			}
			finally
			{
				try
				{
					response?.Dispose();
				}
				catch (Exception e)
				{
					Logger.Log(spider.Identity, $"Close response fail: {e}", Level.Error, e);
				}
			}
		}

		private Page CreateRetryPage(Exception e, Request request, ISpider spider)
		{
			Page page = spider.Site.CycleRetryTimes > 0 ? Spider.AddToCycleRetry(request, spider.Site) : new Page(request);
			if (page != null)
			{
				page.Exception = e;
			}

			Logger.Log(spider.Identity, $"Download {request.Url} failed: {e.Message}.", Level.Warn);
			return page;
		}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request, Site site)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method ?? HttpMethod.Get, request.Url);

			var userAgentHeader = "User-Agent";
			httpRequestMessage.Headers.Add(userAgentHeader, site.Headers.ContainsKey(userAgentHeader) ? site.Headers[userAgentHeader] : site.UserAgent);

			if (!string.IsNullOrWhiteSpace(request.Referer))
			{
				httpRequestMessage.Headers.Add("Referer", request.Referer);
			}

			if (!string.IsNullOrWhiteSpace(request.Origin))
			{
				httpRequestMessage.Headers.Add("Origin", request.Origin);
			}

			if (!string.IsNullOrWhiteSpace(site.Accept))
			{
				httpRequestMessage.Headers.Add("Accept", site.Accept);
			}

			var contentTypeHeader = "Content-Type";

			foreach (var header in site.Headers)
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

			if (httpRequestMessage.Method == HttpMethod.Post)
			{
				var data = string.IsNullOrWhiteSpace(site.EncodingName) ? Encoding.UTF8.GetBytes(request.PostBody) : site.Encoding.GetBytes(request.PostBody);
				httpRequestMessage.Content = new StreamContent(new MemoryStream(data));


				if (site.Headers.ContainsKey(contentTypeHeader))
				{
					httpRequestMessage.Content.Headers.TryAddWithoutValidation(contentTypeHeader, site.Headers[contentTypeHeader]);
				}

				var xRequestedWithHeader = "X-Requested-With";
				if (site.Headers.ContainsKey(xRequestedWithHeader) && site.Headers[xRequestedWithHeader] == "NULL")
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

		private Page HandleResponse(Request request, HttpResponseMessage response, Site site)
		{
			string content = ReadContent(site, response);

			if (_decodeHtml)
			{
#if NET45
				content = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(content), string.IsNullOrEmpty(site.EncodingName) ? Encoding.Default : site.Encoding);
#else
				content = System.Net.WebUtility.UrlDecode(System.Net.WebUtility.HtmlDecode(content));
#endif
			}

			Page page = new Page(request)
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
			if (string.IsNullOrWhiteSpace(site.EncodingName))
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

		private Page SaveFile(Request request, HttpResponseMessage response, ISpider spider)
		{
			var intervalPath = new Uri(request.Url).LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
			string filePath = $"{_downloadFolder}{Env.PathSeperator}{spider.Identity}{intervalPath}";
			if (!File.Exists(filePath))
			{
				try
				{
					string folder = Path.GetDirectoryName(filePath);
					if (!string.IsNullOrWhiteSpace(folder))
					{
						if (!Directory.Exists(folder))
						{
							Directory.CreateDirectory(folder);
						}
					}

					File.WriteAllBytes(filePath, response.Content.ReadAsByteArrayAsync().Result);
				}
				catch (Exception e)
				{
					Logger.Log(spider.Identity, "Storage file failed.", Level.Error, e);
				}
			}
			Logger.Log(spider.Identity, $"Storage file: {request.Url} success.", Level.Info);
			return new Page(request) { Skip = true };
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
