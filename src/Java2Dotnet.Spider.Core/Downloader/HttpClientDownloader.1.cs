using System;
using System.Collections.Generic;
using System.IO;
#if !NET_CORE
using System.Web;
#endif
using System.Text;
using HtmlAgilityPack;
using Java2Dotnet.Spider.Common;
using System.Net.Http;
using System.Net;
using Java2Dotnet.Spider.Core.Proxy;
using System.Threading.Tasks;
using System.Threading;
using Java2Dotnet.Spider.Core.Utils;

namespace Java2Dotnet.Spider.Core.Downloader
{
	/// <summary>
	/// The http downloader based on HttpClient.
	/// </summary>
	public class HttpClientDownloader : BaseDownloader
	{
		public Action<Site, Request> PostBodyGenerator;
		public bool DecodeContentAsUrl;

		HttpClient httpClient = new HttpClient(new GlobalRedirectHandler(new HttpClientHandler()
		{
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseCookies = false,
#if TEST
			UseProxy = true,
#else
			UseProxy = false
#endif
		}));

		public override Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			Site site = spider.Site;

			var acceptStatCodes = site.AcceptStatCode;

			//Logger.InfoFormat("Downloading page {0}", request.Url);

			HttpResponseMessage response = null;
			var proxy = site.GetHttpProxyFromPool();
			request.PutExtra(Request.Proxy, proxy);
			int statusCode = 200;
			try
			{
				PostBodyGenerator?.Invoke(spider.Site, request);

				var httpMessage = GenerateHttpRequestMessage(request, site);

				response = NetworkProxyManager.Current.Execute("http", (m) =>
				{
					var message = (HttpRequestMessage)m;
					return httpClient.SendAsync(message).Result;
				}, httpMessage);

				response.EnsureSuccessStatusCode();
				if (!site.AcceptStatCode.Contains(response.StatusCode))
				{
					throw new DownloadException($"下载 {request.Url} 失败. Code: {response.StatusCode}");
				}
				statusCode = (int)response.StatusCode;
				request.PutExtra(Request.StatusCode, statusCode);

				Page page = HandleResponse(request, response, statusCode, site);

				// need update
				page.TargetUrl = request.Url.ToString();

				//page.SetRawText(File.ReadAllText(@"C:\Users\Lewis\Desktop\taobao.html"));

				// 这里只要是遇上登录的, 则在拨号成功之后, 全部抛异常在Spider中加入Scheduler调度
				// 因此如果使用多线程遇上多个Warning Custom Validate Failed不需要紧张, 可以考虑用自定义Exception分开

				Handle(page, spider);

				// 结束后要置空, 这个值存到Redis会导致无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);

				//#if !NET_CORE
				//					httpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
				//#endif

				return page;

				//正常结果在上面已经Return了, 到此处必然是下载失败的值.
				//throw new SpiderExceptoin("Download failed.");
			}
			catch (DownloadException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request, site.ContentType) { Exception = e };

				Handle(page, spider);
				throw;
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
					Logger.Warn(LogInfo.Create("Close response fail.", spider), e);
				}
			}
		}

		private bool StatusAccept(ICollection<int> acceptStatCode, int statusCode)
		{
			return acceptStatCode.Contains(statusCode);
		}

		//private HttpWebRequest GeneratorCookie(HttpWebRequest httpWebRequest, Site site)
		//{
		//	StringBuilder builder = new StringBuilder();
		//	foreach (var cookie in site.AllCookies)
		//	{
		//		builder.Append($"{cookie.Key}={cookie.Value};");
		//	}
		//	httpWebRequest.Headers.Add("Cookie", builder.ToString());

		//	return httpWebRequest;
		//}

		private HttpRequestMessage GenerateHttpRequestMessage(Request request, Site site)
		{
			if (site == null) return null;

			HttpRequestMessage httpWebRequest = CreateRequestMessage(request);
			if (!site.Headers.ContainsKey("Content-Type") && (site.Headers.ContainsKey("Content-Type") && site.Headers["Content-Type"] != "NULL"))
			{
				httpWebRequest.Headers.Add("ContentType", "application /x-www-form-urlencoded; charset=UTF-8");
			}
			else
			{
				//httpWebRequest.Content.Headers.Add("Content-Type", site.Headers["Content-Type"]);
			}
			if (site.Headers.ContainsKey("UserAgent"))
			{
				httpWebRequest.Headers.Add("UserAgent", site.Headers["UserAgent"]);
			}
			else
			{
				httpWebRequest.Headers.Add("User-Agent", site.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
			}

			if (!string.IsNullOrEmpty(request.Referer))
			{
				httpWebRequest.Headers.Add("Referer", request.Referer);
			}

			httpWebRequest.Headers.Add("Accept", site.Accept ?? "application/json, text/javascript, */*; q=0.01");

			if (!site.Headers.ContainsKey("Accept-Language"))
			{
				httpWebRequest.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
			}

			if (site.IsUseGzip)
			{
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip");
			}


			//httpWebRequest.Timeout = site.Timeout;
			//httpWebRequest.ReadWriteTimeout = site.Timeout;
			//httpWebRequest.AllowAutoRedirect = true;

			// headers
			if (site.Headers != null)
			{
				foreach (var header in site.Headers)
				{
					if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value) && header.Key != "Content-Type" && header.Key != "User-Agent")
					{
						httpWebRequest.Headers.Add(header.Key, header.Value);
					}
				}
			}
			httpWebRequest.Headers.Add("Cookie", site.Cookie);

			if (httpWebRequest.Method == HttpMethod.Post)
			{
				var data = string.IsNullOrEmpty(site.EncodingName) ? Encoding.UTF8.GetBytes(request.PostBody) : site.Encoding.GetBytes(request.PostBody);
				httpWebRequest.Content = new StreamContent(new MemoryStream(data));

				if (site.Headers.ContainsKey("Content-Type"))
				{
					httpWebRequest.Content.Headers.Add("Content-Type", site.Headers["Content-Type"]);
				}

				httpWebRequest.Content.Headers.Add("X-Requested-With", "XMLHttpRequest");
			}
#if !NET_CORE
			//if (site.HttpProxyPoolEnable)
			//{
			//	HttpHost host = site.GetHttpProxyFromPool();
			//	httpWebRequest.Proxy = new WebProxy(host.Host, host.Port);
			//	request.PutExtra(Request.Proxy, host);
			//}
			//else
			//{
			//	// 避开Fiddler之类的代理
			//	httpWebRequest.Proxy = null;
			//}
#else
			//httpWebRequest.Proxy = null;
#endif
			return httpWebRequest;
		}

		private HttpRequestMessage CreateRequestMessage(Request request)
		{
			if (request.Method == null || request.Method.ToUpper().Equals(HttpConstant.Method.Get))
			{
				return new HttpRequestMessage(HttpMethod.Get, request.Url);
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Post))
			{
				return new HttpRequestMessage(HttpMethod.Post, request.Url);
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Head))
			{
				return new HttpRequestMessage(HttpMethod.Head, request.Url);
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Put))
			{
				return new HttpRequestMessage(HttpMethod.Put, request.Url);
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Delete))
			{
				return new HttpRequestMessage(HttpMethod.Delete, request.Url);
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Trace))
			{
				return new HttpRequestMessage(HttpMethod.Trace, request.Url);
			}
			throw new ArgumentException("Illegal HTTP Method " + request.Method);
		}

		private Page HandleResponse(Request request, HttpResponseMessage response, int statusCode, Site site)
		{
			string content = GetContent(site, response);

			if (DecodeContentAsUrl)
			{
#if !NET_CORE
				content = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(content), string.IsNullOrEmpty(site.EncodingName) ? Encoding.Default : site.Encoding);
#else
				content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content));
#endif
			}

			Page page = new Page(request, site.ContentType);
			page.Content = content;
			page.Url = request.Url.ToString();
			page.StatusCode = statusCode;
			foreach (var header in response.Headers)
			{
				page.Request.PutExtra(header.Key, header.Value);
			}

			return page;
		}

		private string GetContent(Site site, HttpResponseMessage response)
		{
			if (string.IsNullOrEmpty(site.EncodingName))
			{
				return response.Content.ReadAsStringAsync().Result;
			}
			else
			{
				byte[] contentBytes = response.Content.ReadAsByteArrayAsync().Result;
				Encoding htmlCharset = Encoding.GetEncoding(site.EncodingName);
				return htmlCharset.GetString(contentBytes);
			}
		}

		//private Encoding GetHtmlCharset(byte[] contentBytes)
		//{
		//	//// charset
		//	//// 1、encoding in http header Content-Type
		//	//string value = contentType;
		//	//var encoding = UrlUtils.GetEncoding(value);
		//	//if (encoding != null)
		//	//{
		//	//	return encoding;
		//	//}
		//	// use default charset to decode first time
		//	Encoding defaultCharset = Encoding.UTF8;
		//	string content = defaultCharset.GetString(contentBytes);
		//	string charset = null;
		//	// 2、charset in meta
		//	if (!string.IsNullOrEmpty(content))
		//	{
		//		HtmlDocument document = new HtmlDocument();
		//		document.LoadHtml(content);
		//		HtmlNodeCollection links = document.DocumentNode.SelectNodes("//meta");
		//		if (links != null)
		//		{
		//			foreach (var link in links)
		//			{
		//				// 2.1、html4.01 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
		//				string metaContent = link.Attributes["content"] != null ? link.Attributes["content"].Value : "";
		//				string metaCharset = link.Attributes["charset"] != null ? link.Attributes["charset"].Value : "";
		//				if (metaContent.IndexOf("charset", StringComparison.Ordinal) != -1)
		//				{
		//					metaContent = metaContent.Substring(metaContent.IndexOf("charset", StringComparison.Ordinal), metaContent.Length - metaContent.IndexOf("charset", StringComparison.Ordinal));
		//					charset = metaContent.Split('=')[1];
		//					break;
		//				}
		//				// 2.2、html5 <meta charset="UTF-8" />
		//				if (!string.IsNullOrEmpty(metaCharset))
		//				{
		//					charset = metaCharset;
		//					break;
		//				}
		//			}
		//		}
		//	}

		//	// 3、todo use tools as cpdetector for content decode
		//	try
		//	{
		//		return Encoding.GetEncoding(string.IsNullOrEmpty(charset) ? "UTF-8" : charset);
		//	}
		//	catch
		//	{
		//		return Encoding.UTF8;
		//	}
		//}
	}

	public class GlobalRedirectHandler : DelegatingHandler
	{
		public GlobalRedirectHandler(HttpMessageHandler innerHandler)
		{
			InnerHandler = innerHandler;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<HttpResponseMessage>();

			base.SendAsync(request, cancellationToken)
				.ContinueWith(t =>
				{
					HttpResponseMessage response;
					try
					{
						response = t.Result;
					}
					catch (Exception e)
					{
						response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
						response.ReasonPhrase = e.Message;
					}
					if (response.StatusCode == HttpStatusCode.MovedPermanently
						|| response.StatusCode == HttpStatusCode.Moved
						|| response.StatusCode == HttpStatusCode.Redirect
						|| response.StatusCode == HttpStatusCode.Found
						|| response.StatusCode == HttpStatusCode.SeeOther
						|| response.StatusCode == HttpStatusCode.RedirectKeepVerb
						|| response.StatusCode == HttpStatusCode.TemporaryRedirect

						|| (int)response.StatusCode == 308)
					{

						var newRequest = CopyRequest(response.RequestMessage);

						if (response.StatusCode == HttpStatusCode.Redirect
							|| response.StatusCode == HttpStatusCode.Found
							|| response.StatusCode == HttpStatusCode.SeeOther)
						{
							newRequest.Content = null;
							newRequest.Method = HttpMethod.Get;

						}
						newRequest.RequestUri = response.Headers.Location;

						base.SendAsync(newRequest, cancellationToken)
							.ContinueWith(t2 => tcs.SetResult(t2.Result));
					}
					else
					{
						tcs.SetResult(response);
					}
				});

			return tcs.Task;
		}

		private static HttpRequestMessage CopyRequest(HttpRequestMessage oldRequest)
		{
			var newrequest = new HttpRequestMessage(oldRequest.Method, oldRequest.RequestUri);

			foreach (var header in oldRequest.Headers)
			{
				newrequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
			foreach (var property in oldRequest.Properties)
			{
				newrequest.Properties.Add(property);
			}
			if (oldRequest.Content != null) newrequest.Content = new StreamContent(oldRequest.Content.ReadAsStreamAsync().Result);
			return newrequest;
		}
	}
}
