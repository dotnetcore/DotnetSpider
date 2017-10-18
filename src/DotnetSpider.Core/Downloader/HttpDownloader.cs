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
using System.Linq;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// The http downloader
	/// </summary>
	public class HttpDownloader : BaseDownloader
	{
		private static readonly List<string> MediaTypes = new List<string>
		{
			"text/html",
			"text/plain",
			"text/richtext",
			"text/xml",
			"text/json",
			"text/javascript",
			"application/soap+xml",
			"application/xml",
			"application/json",
			"application/x-javascript",
			"application/javascript",
			"application/x-www-form-urlencoded"
		};

		public bool DecodeHtml { get; set; }

		protected override Page DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;

			HttpWebResponse response = null;
			var proxy = site.GetHttpProxy();

			try
			{
				var httpMessage = GenerateHttpWebRequest(request, site);

				response = NetworkCenter.Current.Execute("http", () =>
				{
					return (HttpWebResponse)httpMessage.GetResponse();
				});

				request.StatusCode = response.StatusCode;

				if (!site.AcceptStatCode.Contains(response.StatusCode))
				{
					throw new DownloadException($"Download {request.Url} failed. Code {response.StatusCode}");
				}
				Page page;

				var mediaType = response.ContentType?.Split(';').FirstOrDefault();
				if (!string.IsNullOrEmpty(mediaType) && !MediaTypes.Contains(mediaType))
				{
					if (!site.DownloadFiles)
					{
						Logger.MyLog(spider.Identity, $"Miss request: {request.Url} because media type is not text.", LogLevel.Error);
						return new Page(request, null) { Skip = true };
					}
					else
					{
						page = SaveFile(request, response, spider);
					}
				}
				else
				{
					page = ConstructPage(request, response, site);
				}

				if (string.IsNullOrEmpty(page.Content))
				{
					Logger.MyLog(spider.Identity, $"Content is empty: {request.Url}.", LogLevel.Warn);
				}

				page.TargetUrl = response.ResponseUri.ToString();

				return page;
			}
			catch (DownloadException de)
			{
				Page page = site.CycleRetryTimes > 0 ? Spider.AddToCycleRetry(request, site) : new Page(request, null);

				if (page != null)
				{
					page.Exception = de;
				}
				Logger.MyLog(spider.Identity, $"Download {request.Url} failed: {de.Message}", LogLevel.Warn);

				return page;
			}
			catch (WebException he)
			{
				Page page = site.CycleRetryTimes > 0 ? Spider.AddToCycleRetry(request, site) : new Page(request, null);
				if (page != null)
				{
					page.Exception = he;
				}

				Logger.MyLog(spider.Identity, $"Download {request.Url} failed: {he.Message}.", LogLevel.Warn);
				return page;
			}
			catch (Exception e)
			{
				Page page = new Page(request, null)
				{
					Exception = e,
					Skip = true
				};

				Logger.MyLog(spider.Identity, $"Download {request.Url} failed: {e.Message}.", LogLevel.Error, e);
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
					Logger.MyLog(spider.Identity, "Close response fail.", LogLevel.Error, e);
				}
			}
		}

		private HttpWebRequest GenerateHttpWebRequest(Request request, Site site)
		{
			if (site == null) return null;
			if (site.Headers == null)
			{
				site.Headers = new Dictionary<string, string>();
			}
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(request.Url);
			httpWebRequest.Method = request.Method.Method;

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

			if (httpWebRequest.Method == "POST")
			{
				var data = string.IsNullOrEmpty(site.EncodingName) ? Encoding.UTF8.GetBytes(request.PostBody) : site.Encoding.GetBytes(request.PostBody);

				if (site.Headers.ContainsKey("Content-Type"))
				{
					httpWebRequest.Headers.Add("Content-Type", site.Headers["Content-Type"]);
				}

				if (site.Headers.ContainsKey("X-Requested-With") && site.Headers["X-Requested-With"] == "NULL")
				{
					httpWebRequest.Headers.Remove("X-Requested-With");
				}
				else
				{
					if (!httpWebRequest.Headers.AllKeys.Contains("X-Requested-With") && !httpWebRequest.Headers.AllKeys.Contains("X-Requested-With"))
					{
						httpWebRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
					}
				}

				byte[] buffer = Encoding.UTF8.GetBytes(request.PostBody);

				if (buffer != null)
				{
					httpWebRequest.ContentLength = buffer.Length;
					httpWebRequest.GetRequestStream().Write(buffer, 0, buffer.Length);
				}
			}
			httpWebRequest.Timeout = site.Timeout;

			return httpWebRequest;
		}

		private Page ConstructPage(Request request, HttpWebResponse response, Site site)
		{
			string content = ReadContent(site, response);

			if (DecodeHtml)
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

		private string ReadContent(Site site, HttpWebResponse response)
		{
			MemoryStream memoryStream = new MemoryStream(0x1000);
			using (Stream responseStream = response.GetResponseStream())
			{
				byte[] buffer = new byte[0x1000];
				int bytes;
				while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					memoryStream.Write(buffer, 0, bytes);
				}
			}

			byte[] contentBytes = memoryStream.StreamToBytes();
			contentBytes = PreventCutOff(contentBytes);
			if (string.IsNullOrEmpty(site.EncodingName))
			{
				var charSet = response.CharacterSet;
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
