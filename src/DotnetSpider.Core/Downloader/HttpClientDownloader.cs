using System;
using System.Collections.Generic;
using System.IO;
#if !NET_CORE
using System.Web;
#endif
using System.Text;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;
using NLog;
using DotnetSpider.Core.Redial;

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
        private static HashSet<string> MediaTypes = new HashSet<string>
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
        private readonly string _downloadFolder;
        private bool _decodeHtml { get; set; }

		public HttpClientDownloader()
		{
			_httpClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = false
			});

            _downloadFolder = Path.Combine(Env.BaseDirectory, "download");
        }

		public HttpClientDownloader(int timeout=8) : this()
		{
			_httpClient.Timeout = new TimeSpan(0, 0, timeout);
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 构造方法
        /// </summary>
        /// <param name="timeout">下载超时时间 Download timeout.</param>
        /// <param name="decodeHtml">下载的内容是否需要HTML解码 Whether <see cref="Page.Content"/> need to Html Decode.</param>
        public HttpClientDownloader(int timeout = 8, bool decodeHtml = false) : this()
        {
            _httpClient.Timeout = new TimeSpan(0, 0, timeout);
            _decodeHtml = decodeHtml;
        }


        public static void AddMediaTypes(string type)
		{
			MediaTypes.Add(type);
		}
        /// <summary>
        /// Http download implemention
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// HTTP下载的实现
        /// </summary>
        /// <param name="request">请求信息 <see cref="Request"/></param>
        /// <param name="spider">爬虫 <see cref="ISpider"/></param>
        /// <returns>页面数据 <see cref="Page"/></retur
        protected override async Task<Page> DowloadContent(Request request, ISpider spider)
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
                        return await Task.FromResult(new Page(request) { Skip = true });
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



        private Page SaveFile(Request request, HttpResponseMessage response, ISpider spider)
        {
            var intervalPath = request.Url.LocalPath.Replace("//", "/").Replace("/", Env.PathSeperator);
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
                catch
                {
                    Logger.Error(spider.Identity, "Storage file failed.");
                }
            }
            Logger.Info($"Storage file: {request.Url} success.");
            return new Page(request) { Skip = true };
        }

        /// <summary>
        /// 替换空格
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
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
