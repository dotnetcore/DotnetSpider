using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotnetSpider.Common;
using DotnetSpider.Proxy;

namespace DotnetSpider.Downloader
{
	public class HttpWebRequestDownloader : BaseDownloader
	{
		private readonly string _downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");
		private readonly int _timeout;
		private readonly bool _decodeHtml;

		public WebProxy FiddlerProxy { get; set; } = new WebProxy("http://127.0.0.1:8888");
		public bool UseFiddlerProxy { get; set; } = false;

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

			if (request.Site.DownloadFiles)
			{
				string filePath = CreateFilePath(request);
				if (File.Exists(filePath))
				{
					Logger.Information($"File {request.Url} already exists.");
					return response;
				}
			}

			var httpWebRequest = GenerateHttpWebRequest(request);
			HttpWebResponse httpWebResponse = null;

			try
			{
				httpWebResponse =
					NetworkCenter.Current.Execute("downloader", () => (HttpWebResponse)httpWebRequest.GetResponse());
				response.StatusCode = httpWebResponse.StatusCode;
				EnsureSuccessStatusCode(response.StatusCode);
				response.TargetUrl = httpWebResponse.ResponseUri.ToString();

				if (!request.Site.ExcludeMediaTypes.Any(t => httpWebResponse.ContentType.Contains(t)))
				{
					if (!request.Site.DownloadFiles)
					{
						Logger.Warning($"Ignore {request.Url} because media type is not allowed to download.");
					}
					else
					{
						StorageFile(request, httpWebResponse);
					}
				}
				else
				{
					string content = ReadContent(httpWebResponse, request.Site);

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
				var proxy = httpWebRequest.Proxy as WebProxy;
				if (HttpProxyPool.Instance != null && proxy != null && httpWebRequest.Proxy != FiddlerProxy)
				{
					HttpProxyPool.Instance.ReturnProxy(proxy, HttpStatusCode.Accepted);
				}
				try
				{
					httpWebResponse?.Close();
				}
				catch (Exception e)
				{
					throw new BypassedDownloaderException($"Close response {request.Url} failed: {e.Message}");
				}
			}

			return response;
		}

		protected virtual string ReadContent(HttpWebResponse response, Site site)
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
			if (string.IsNullOrEmpty(site.EncodingName))
			{
				Encoding htmlCharset = EncodingExtensions.GetEncoding(response.CharacterSet, contentBytes);
				return htmlCharset.GetString(contentBytes, 0, contentBytes.Length);
			}

			return Encoding.GetEncoding(site.EncodingName).GetString(contentBytes, 0, contentBytes.Length);
		}

		private HttpWebRequest GenerateHttpWebRequest(Request request)
		{
			var site = request.Site;
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(request.Url);
			httpWebRequest.Method = request.Method.ToString().ToUpper();

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

			if (HttpMethod.Post == request.Method)
			{
				var bytes = string.IsNullOrEmpty(site.EncodingName)
					? Encoding.UTF8.GetBytes(request.Content)
					: Encoding.GetEncoding(site.EncodingName).GetBytes(request.Content);
				var requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);

				if (site.Headers.ContainsKey(contentTypeHeader))
				{
					httpWebRequest.Headers.Add(contentTypeHeader, site.Headers[contentTypeHeader]);
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
			}
			else
			{
				httpWebRequest.Proxy = null;
			}

			httpWebRequest.AllowAutoRedirect = AllowAutoRedirect;
			httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			httpWebRequest.CookieContainer = CookieContainer;
			httpWebRequest.Timeout = _timeout;

			if (UseFiddlerProxy && FiddlerProxy != null)
			{
				httpWebRequest.Proxy = FiddlerProxy;
			}
			return httpWebRequest;
		}

		private void StorageFile(Request request, HttpWebResponse response)
		{
			string filePath = CreateFilePath(request);
			if (!File.Exists(filePath))
			{
				try
				{
					string folder = Path.GetDirectoryName(filePath);
					if (!string.IsNullOrEmpty(folder))
					{
						if (!Directory.Exists(folder))
						{
							Directory.CreateDirectory(folder);
						}

						var bytes = StreamExtensions.ConvertToBytes(response.GetResponseStream());
						File.WriteAllBytes(filePath, bytes);
						Logger.Information($"Storage file {request.Url} success.");
					}
					else
					{
						throw new DownloaderException($"Can not create folder for file path {filePath}.");
					}
				}
				catch (Exception e)
				{
					Logger.Error($"Storage file {request.Url} failed: {e.Message}.");
				}
			}
			else
			{
				Logger.Information($"File {request.Url} already exists.");
			}
		}

		private string CreateFilePath(Request request)
		{
			var uri = new Uri(request.Url);
			var intervalPath = (uri.Host + uri.LocalPath).Replace("//", "/").Replace("/", DownloaderEnv.PathSeperator);
			string filePath = $"{_downloadFolder}{DownloaderEnv.PathSeperator}{intervalPath}";
			return filePath;
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

		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true; //总是接受  
		}
	}
}