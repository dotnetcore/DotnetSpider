

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Net;
//#if !NET_CORE
//using System.Runtime.Remoting.Contexts;
//using System.Web;
//#endif

//using System.Text;

//using HtmlAgilityPack;
//using DotnetSpider.Core.Common;
//using DotnetSpider.Core.Proxy;
//using DotnetSpider.Core.Utils;
//using DotnetSpider.Redial;

//namespace DotnetSpider.Core.Downloader
//{
//	/// <summary>
//	/// The http downloader based on HttpClient.
//	/// </summary>
//	public class HttpClientDownloader : BaseDownloader
//	{
//		//private static AutomicLong _exceptionCount = new AutomicLong(0);
//		public Action<Site, Request> GeneratePostBody;
//		public bool DecodeContentAsUrl;

//		public override Page Download(Request request, ISpider spider)
//		{
//			if (spider.Site == null)
//			{
//				return null;
//			}

//			Site site = spider.Site;

//			ICollection<int> acceptStatCode = site.AcceptStatCode;
//			var charset = site.Encoding;

//			//Logger.InfoFormat("Downloading page {0}", request.Url);

//			int statusCode = 0;

//			HttpWebResponse response = null;
//			try
//			{
//				if (GeneratePostBody != null)
//				{
//					SingleExecutor.Execute(() =>
//					{
//						GeneratePostBody(spider.Site, request);
//					});
//				}

//				var httpWebRequest = GetHttpWebRequest(request, site);

//				response = RedialManagerUtils.Execute("downloader-download", h =>
//				{
//					HttpWebRequest tmpHttpWebRequest = (HttpWebRequest)h;

//					if (HttpConstant.Method.Post.Equals(request.Method) && !string.IsNullOrEmpty(request.PostBody))
//					{
//						var data = spider.Site.Encoding.GetBytes(request.PostBody);
//#if !NET_CORE
//						tmpHttpWebRequest.ContentLength = data.Length;

//						using (Stream newStream = tmpHttpWebRequest.GetRequestStream())
//						{
//							newStream.Write(data, 0, data.Length);
//							newStream.Close();
//						}
//#else
//						using (Stream newStream = tmpHttpWebRequest.GetRequestStreamAsync().Result)
//						{
//							newStream.Write(data, 0, data.Length);
//							newStream.Dispose();
//						}
//#endif
//					}

//#if !NET_CORE
//					return (HttpWebResponse)tmpHttpWebRequest?.GetResponse();
//#else
//					return (HttpWebResponse)tmpHttpWebRequest?.GetResponseAsync().Result;
//#endif

//				}, httpWebRequest);

//				statusCode = (int)response.StatusCode;
//				request.PutExtra(Request.StatusCode, statusCode);
//				if (StatusAccept(acceptStatCode, statusCode))
//				{
//					Page page = HandleResponse(request, charset, response, statusCode, site);

//					//page.SetRawText(File.ReadAllText(@"C:\Users\Lewis\Desktop\taobao.html"));

//					// 这里只要是遇上登录的, 则在拨号成功之后, 全部抛异常在Spider中加入Scheduler调度
//					// 因此如果使用多线程遇上多个Warning Custom Validate Failed不需要紧张, 可以考虑用自定义Exception分开
//					ValidatePage(page);

//					// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
//					request.PutExtra(Request.CycleTriedTimes, null);

//#if !NET_CORE
//					httpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
//#endif

//					return page;
//				}
//				else
//				{
//					throw new SpiderExceptoin("Download failed.");
//				}

//				//正常结果在上面已经Return了, 到此处必然是下载失败的值.
//				//throw new SpiderExceptoin("Download failed.");
//			}
//			catch (RedialException)
//			{
//				throw;
//			}
//			catch (Exception e)
//			{
//				Page page = new Page(request, site.ContentType) { Exception = e };

//				ValidatePage(page);
//				throw;
//			}
//			finally
//			{
//				// 先Close Response, 避免前面语句异常导致没有关闭.
//				try
//				{
//					//ensure the connection is released back to pool
//					//check:
//					//EntityUtils.consume(httpResponse.getEntity());
//#if !NET_CORE
//					response?.Close();
//#else
//					response?.Dispose();
//#endif
//				}
//				catch (Exception e)
//				{
//					Logger.Warn("Close response fail.", e);
//				}
//				request.PutExtra(Request.StatusCode, statusCode);
//			}
//		}

//		private bool StatusAccept(ICollection<int> acceptStatCode, int statusCode)
//		{
//			return acceptStatCode.Contains(statusCode);
//		}

//		//private HttpWebRequest GeneratorCookie(HttpWebRequest httpWebRequest, Site site)
//		//{
//		//	StringBuilder builder = new StringBuilder();
//		//	foreach (var cookie in site.AllCookies)
//		//	{
//		//		builder.Append($"{cookie.Key}={cookie.Value};");
//		//	}
//		//	httpWebRequest.Headers.Add("Cookie", builder.ToString());

//		//	return httpWebRequest;
//		//}

//		private HttpWebRequest GetHttpWebRequest(Request request, Site site)
//		{
//			if (site == null) return null;

//			HttpWebRequest httpWebRequest = SelectRequestMethod(request);
//			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";


//#if !NET_CORE
//			httpWebRequest.UserAgent = site.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
//			httpWebRequest.Referer = request.Referer ?? "";
//			if (site.IsUseGzip)
//			{
//				httpWebRequest.Headers.Add("Accept-Encoding", "gzip");
//			}
//			httpWebRequest.Timeout = site.Timeout;
//			httpWebRequest.ReadWriteTimeout = site.Timeout;
//			httpWebRequest.AllowAutoRedirect = true;
//#else
//			httpWebRequest.Headers["UserAgent"] = site.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
//			httpWebRequest.Headers["Referer"] = request.Referer ?? "";
//			if (site.IsUseGzip)
//			{
//				httpWebRequest.Headers["Accept-Encoding"] = "gzip";
//			}
//#endif
//			httpWebRequest.Accept = site.Accept ?? "application/json, text/javascript, */*; q=0.01";

//#if !NET_CORE
//			httpWebRequest.ServicePoint.Expect100Continue = false;
//#endif



//			// headers
//			if (site.Headers != null)
//			{
//				foreach (var header in site.Headers)
//				{
//#if !NET_CORE
//					httpWebRequest.Headers.Add(header.Key, header.Value);
//#else
//					httpWebRequest.Headers[header.Key] = header.Value;
//#endif
//				}
//			}
//			httpWebRequest.Headers["Cookie"] = site.Cookie;


//			httpWebRequest.ContinueTimeout = site.Timeout;

//#if !NET_CORE
//			if (site.HttpProxyPoolEnable)
//			{
//				HttpHost host = site.GetHttpProxyFromPool();
//				httpWebRequest.Proxy = new WebProxy(host.Host, host.Port);
//				request.PutExtra(Request.Proxy, host);
//			}
//			else
//			{
//				// 避开Fiddler之类的代理
//				httpWebRequest.Proxy = null;
//			}
//#else
//			httpWebRequest.Proxy = null;
//#endif
//			return httpWebRequest;
//		}

//		private HttpWebRequest SelectRequestMethod(Request request)
//		{
//			HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(request.Url);
//			if (request.Method == null || request.Method.ToUpper().Equals(HttpConstant.Method.Get))
//			{
//				//default get
//				webrequest.Method = HttpConstant.Method.Get;
//				return webrequest;
//			}
//			if (request.Method.ToUpper().Equals(HttpConstant.Method.Post))
//			{
//				webrequest.Method = HttpConstant.Method.Post;
//				//webrequest.Headers["X-Requested-With"] = "XMLHttpRequest";
//				return webrequest;
//			}
//			if (request.Method.ToUpper().Equals(HttpConstant.Method.Head))
//			{
//				webrequest.Method = HttpConstant.Method.Head;
//				return webrequest;
//			}
//			if (request.Method.ToUpper().Equals(HttpConstant.Method.Put))
//			{
//				webrequest.Method = HttpConstant.Method.Put;
//				return webrequest;
//			}
//			if (request.Method.ToUpper().Equals(HttpConstant.Method.Delete))
//			{
//				webrequest.Method = HttpConstant.Method.Delete;
//				return webrequest;
//			}
//			if (request.Method.ToUpper().Equals(HttpConstant.Method.Trace))
//			{
//				webrequest.Method = HttpConstant.Method.Trace;
//				return webrequest;
//			}
//			throw new ArgumentException("Illegal HTTP Method " + request.Method);
//		}

//		private Page HandleResponse(Request request, Encoding charset, HttpWebResponse response, int statusCode, Site site)
//		{
//			string content = GetContent(charset, response);
//			if (string.IsNullOrEmpty(content))
//			{
//				throw new SpiderExceptoin($"Download {request.Url} failed.");
//			}

//			if (DecodeContentAsUrl)
//			{
//#if !NET_CORE
//				content = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(content), charset);
//#else
//				content = WebUtility.UrlDecode(WebUtility.HtmlDecode(content));
//#endif
//			}

//			Page page = new Page(request, site.ContentType);
//			page.Content = content;
//			page.TargetUrl = response.ResponseUri.ToString();
//			page.Url = request.Url.ToString();
//			page.StatusCode = statusCode;
//			foreach (string key in response.Headers.AllKeys)
//			{
//				page.Request.PutExtra(key, response.Headers[key]);
//			}

//			return page;
//		}

//		private string GetContent(Encoding charset, HttpWebResponse response)
//		{
//			byte[] contentBytes = GetContentBytes(response);

//			if (charset == null)
//			{
//				Encoding htmlCharset = GetHtmlCharset(response.ContentType, contentBytes);
//				if (htmlCharset != null)
//				{
//					return htmlCharset.GetString(contentBytes);
//				}

//				return Encoding.UTF8.GetString(contentBytes);
//			}
//			return charset.GetString(contentBytes);
//		}

//		private byte[] GetContentBytes(HttpWebResponse response)
//		{
//			Stream stream = null;
//			bool isGizp = false;

//#if !NET_CORE
//			isGizp = response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase);
//#else
//			var contentEncodingHeader = response.Headers["ContentEncoding"];
//			if (contentEncodingHeader != null && contentEncodingHeader.Contains("gzip"))
//			{
//				isGizp = true;
//			}
//#endif
//			//GZIIP处理  
//			if (isGizp)
//			{
//				//开始读取流并设置编码方式
//				var tempStream = response.GetResponseStream();
//				if (tempStream != null) stream = new GZipStream(tempStream, CompressionMode.Decompress);
//			}
//			else
//			{
//				//开始读取流并设置编码方式  
//				stream = response.GetResponseStream();
//			}

//			MemoryStream resultStream = new MemoryStream();
//			if (stream != null)
//			{
//				stream.CopyTo(resultStream);
//				return resultStream.StreamToBytes();
//			}
//			return null;
//		}

//		private Encoding GetHtmlCharset(string contentType, byte[] contentBytes)
//		{
//			// charset
//			// 1、encoding in http header Content-Type
//			string value = contentType;
//			var encoding = UrlUtils.GetEncoding(value);
//			if (encoding != null)
//			{
//				return encoding;
//			}
//			// use default charset to decode first time
//			Encoding defaultCharset = Encoding.UTF8;
//			string content = defaultCharset.GetString(contentBytes);
//			string charset = null;
//			// 2、charset in meta
//			if (!string.IsNullOrEmpty(content))
//			{
//				HtmlDocument document = new HtmlDocument();
//				document.LoadHtml(content);
//				HtmlNodeCollection links = document.DocumentNode.SelectNodes("//meta");
//				if (links != null)
//				{
//					foreach (var link in links)
//					{
//						// 2.1、html4.01 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
//						string metaContent = link.Attributes["content"] != null ? link.Attributes["content"].Value : "";
//						string metaCharset = link.Attributes["charset"] != null ? link.Attributes["charset"].Value : "";
//						if (metaContent.IndexOf("charset", StringComparison.Ordinal) != -1)
//						{
//							metaContent = metaContent.Substring(metaContent.IndexOf("charset", StringComparison.Ordinal), metaContent.Length - metaContent.IndexOf("charset", StringComparison.Ordinal));
//							charset = metaContent.Split('=')[1];
//							break;
//						}
//						// 2.2、html5 <meta charset="UTF-8" />
//						if (!string.IsNullOrEmpty(metaCharset))
//						{
//							charset = metaCharset;
//							break;
//						}
//					}
//				}
//			}

//			// 3、todo use tools as cpdetector for content decode
//			try
//			{
//				return Encoding.GetEncoding(string.IsNullOrEmpty(charset) ? "UTF-8" : charset);
//			}
//			catch
//			{
//				return Encoding.UTF8;
//			}
//		}
//	}
//}
