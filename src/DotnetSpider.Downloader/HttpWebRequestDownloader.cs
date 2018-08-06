using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotnetSpider.Common;
using DotnetSpider.Proxy;
using LZ4;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// Downloader using <see cref="HttpWebRequest"/>
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 纯HTTP下载器
	/// </summary>
	public class HttpWebRequestDownloader : Downloader
	{
		private readonly int _timeout;
		private readonly bool _decodeHtml;

		public HttpWebRequestDownloader(int timeout = 8000, bool decodeHtml = false)
		{
			_timeout = timeout;
			_decodeHtml = decodeHtml;

			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
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

			HttpWebRequest httpWebRequest = null;
			HttpWebResponse httpWebResponse = null;

			try
			{
				httpWebRequest = GenerateHttpWebRequest(request);
				httpWebResponse =
					NetworkCenter.Current.Execute("downloader", () => (HttpWebResponse)httpWebRequest.GetResponse());
				response.StatusCode = httpWebResponse.StatusCode;
				EnsureSuccessStatusCode(response.StatusCode);
				response.TargetUrl = httpWebResponse.ResponseUri.ToString();
				var bytes = ReadResponseStream(httpWebResponse);
				if (!request.Site.ExcludeMediaTypes.Any(t => httpWebResponse.ContentType.Contains(t)))
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
					string content = ReadContent(bytes, httpWebResponse.ContentType, request.Site);

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

					DetectContentType(response, httpWebResponse.ContentType);
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
				if (httpWebRequest != null)
				{
					var proxy = httpWebRequest.Proxy as WebProxy;
					if (HttpProxyPool.Instance != null && proxy != null && httpWebRequest.Proxy != FiddlerProxy)
					{
						HttpProxyPool.Instance.ReturnProxy(proxy, httpWebResponse == null ? HttpStatusCode.ServiceUnavailable : httpWebResponse.StatusCode);
					}
					try
					{
						httpWebResponse.Close();
					}
					catch (Exception e)
					{
						throw new BypassedDownloaderException($"Close response {request.Url} failed: {e.Message}");
					}
				}
			}

			return response;
		}

		protected virtual byte[] ReadResponseStream(HttpWebResponse response)
		{
			var stream = response.GetResponseStream();
			if (stream == null)
			{
				return null;
			}
			byte[] buffer = new byte[1024];
			byte[] contentBytes;
			using (MemoryStream ms = new MemoryStream())
			{
				int readBytes;
				while ((readBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, readBytes);
				}

				contentBytes = ms.ToArray();
			}
			contentBytes = PreventCutOff(contentBytes);
			stream.Dispose();
			return contentBytes;
		}

		private HttpWebRequest GenerateHttpWebRequest(Request request)
		{
			var site = request.Site;
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(request.Url);
			httpWebRequest.Method = request.Method.Method;

			// Add user-agent
			var userAgentHeader = "User-Agent";
			var userAgentHeaderValue = site.Headers.ContainsKey(userAgentHeader)
				? site.Headers[userAgentHeader]
				: site.UserAgent;
			userAgentHeaderValue = string.IsNullOrEmpty(userAgentHeaderValue)
				? "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36"
				: userAgentHeaderValue;
			httpWebRequest.UserAgent = userAgentHeaderValue;

			if (!string.IsNullOrEmpty(request.Referer))
			{
				httpWebRequest.Referer = request.Referer;
			}

			if (!string.IsNullOrEmpty(request.Origin))
			{
				httpWebRequest.Headers.Add("Origin", request.Origin);
			}

			if (!string.IsNullOrEmpty(site.Accept))
			{
				httpWebRequest.Accept = site.Accept;
			}

			var contentTypeHeader = "Content-Type";

			foreach (var header in site.Headers)
			{
				if (header.Key.ToLower() == "cookie")
				{
					continue;
				}

				if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value) &&
					header.Key != contentTypeHeader && header.Key != userAgentHeader)
				{
					httpWebRequest.Headers.Add(header.Key, header.Value);
				}
			}
			// 写入开始后不能再修改此属性
			httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			if (HttpMethod.Post == request.Method)
			{
				var bytes = CompressContent(request);

				var requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);

				if (site.Headers.ContainsKey(contentTypeHeader))
				{
					httpWebRequest.ContentType = site.Headers[contentTypeHeader];
				}


				var xRequestedWithHeader = "X-Requested-With";
				if (site.Headers.ContainsKey(xRequestedWithHeader) && site.Headers[xRequestedWithHeader] == "NULL")
				{
					httpWebRequest.Headers.Remove(xRequestedWithHeader);
				}
				else
				{
					if (string.IsNullOrEmpty(httpWebRequest.Headers.Get(xRequestedWithHeader)))
					{
						httpWebRequest.Headers.Add(xRequestedWithHeader, "XMLHttpRequest");
					}
				}
			}

			if (HttpProxyPool.Instance != null)
			{
				httpWebRequest.Proxy = HttpProxyPool.Instance.GetProxy();
				if (httpWebRequest.Proxy == null)
				{
					throw new DownloaderException("No avaliable proxy.");
				}
			}

			httpWebRequest.AllowAutoRedirect = AllowAutoRedirect;
			httpWebRequest.CookieContainer = CookieContainer;
			httpWebRequest.Timeout = _timeout;

			if (UseFiddlerProxy && FiddlerProxy != null)
			{
				httpWebRequest.Proxy = FiddlerProxy;
			}
			return httpWebRequest;
		}

		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true; //总是接受  
		}
	}
}