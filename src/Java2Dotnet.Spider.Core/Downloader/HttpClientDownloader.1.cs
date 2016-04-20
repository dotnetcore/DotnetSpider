using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
#if !NET_CORE
using System.Runtime.Remoting.Contexts;
using System.Web;
#endif
using System.Text;
using HtmlAgilityPack;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Proxy;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Redial;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Threading;
using System.Linq;

namespace Java2Dotnet.Spider.Core.Downloader
{
	/// <summary>
	/// The http downloader based on HttpClient.
	/// </summary>
	public class HttpClientDownloader : BaseDownloader
	{
		//private static AutomicLong _exceptionCount = new AutomicLong(0);
		public Action<Site, Request> GeneratePostBody;
		public bool DecodeContentAsUrl;

		public override Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			Site site = spider.Site;

			ICollection<int> acceptStatCode = site.AcceptStatCode;

			//Logger.InfoFormat("Downloading page {0}", request.Url);

			HttpResponseMessage response = null;

			try
			{
				if (GeneratePostBody != null)
				{
					SingleExecutor.Execute(() =>
					{
						GeneratePostBody(spider.Site, request);
					});
				}

				var httpMessage = GenerateHttpRequestMessage(request, site);
				HttpClient httpClient = new HttpClient();

				response = RedialManagerUtils.Execute("downloader-download", (m) =>
				{
					var message = (HttpRequestMessage)m;
					httpClient.Timeout = new TimeSpan(0, 0, site.Timeout);
					return httpClient.SendAsync(message).Result;
				}, httpMessage);

				response.EnsureSuccessStatusCode();
				int statusCode = (int)response.StatusCode;
				request.PutExtra(Request.StatusCode, statusCode);

				Page page = HandleResponse(request, response, statusCode, site);

				// need update
				page.TargetUrl = request.Url.ToString();

				//page.SetRawText(File.ReadAllText(@"C:\Users\Lewis\Desktop\taobao.html"));

				// 这里只要是遇上登录的, 则在拨号成功之后, 全部抛异常在Spider中加入Scheduler调度
				// 因此如果使用多线程遇上多个Warning Custom Validate Failed不需要紧张, 可以考虑用自定义Exception分开
				ValidatePage(page);

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				request.PutExtra(Request.CycleTriedTimes, null);

				//#if !NET_CORE
				//					httpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
				//#endif

				return page;

				//正常结果在上面已经Return了, 到此处必然是下载失败的值.
				//throw new SpiderExceptoin("Download failed.");
			}
			catch (RedialException)
			{
				throw;
			}
			catch (Exception e)
			{
				Page page = new Page(request, site.ContentType) { Exception = e };

				ValidatePage(page);
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
					Logger.Warn("Close response fail.", e);
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
			httpWebRequest.Headers.Add("ContentType", "application /x-www-form-urlencoded; charset=UTF-8");
			httpWebRequest.Headers.Add("UserAgent", site.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0");

			if (!string.IsNullOrEmpty(request.Referer))
			{
				httpWebRequest.Headers.Add("Referer", request.Referer);
			}

			httpWebRequest.Headers.Add("Accept", site.Accept ?? "application/json, text/javascript, */*; q=0.01");
			//httpWebRequest.Headers.Add("",);

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
					if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
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
			httpWebRequest.Proxy = null;
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
				Encoding htmlCharset = GetHtmlCharset(contentBytes);
				if (htmlCharset != null)
				{
					return htmlCharset.GetString(contentBytes);
				}

				return Encoding.UTF8.GetString(contentBytes);
				;
			}

		}

		private byte[] GetContentBytes(HttpResponseMessage response)
		{
			Stream stream = null;
			bool isGizp = false;


			//isGizp = response.Headers.GetValues("ContentEncoding").Contains("gzip");

			////GZIIP处理  
			//if (isGizp)
			//{
			//开始读取流并设置编码方式
			//	var tempStream = response.Content.ReadAsStreamAsync().Result;
			//	if (tempStream != null) stream = new GZipStream(tempStream, CompressionMode.Decompress);
			//}
			//else
			{
				//开始读取流并设置编码方式  
				stream = response.Content.ReadAsStreamAsync().Result;
			}

			MemoryStream resultStream = new MemoryStream();
			if (stream != null)
			{
				stream.CopyTo(resultStream);
				return resultStream.StreamToBytes();
			}
			return null;
		}

		private Encoding GetHtmlCharset(byte[] contentBytes)
		{
			//// charset
			//// 1、encoding in http header Content-Type
			//string value = contentType;
			//var encoding = UrlUtils.GetEncoding(value);
			//if (encoding != null)
			//{
			//	return encoding;
			//}
			// use default charset to decode first time
			Encoding defaultCharset = Encoding.UTF8;
			string content = defaultCharset.GetString(contentBytes);
			string charset = null;
			// 2、charset in meta
			if (!string.IsNullOrEmpty(content))
			{
				HtmlDocument document = new HtmlDocument();
				document.LoadHtml(content);
				HtmlNodeCollection links = document.DocumentNode.SelectNodes("//meta");
				if (links != null)
				{
					foreach (var link in links)
					{
						// 2.1、html4.01 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
						string metaContent = link.Attributes["content"] != null ? link.Attributes["content"].Value : "";
						string metaCharset = link.Attributes["charset"] != null ? link.Attributes["charset"].Value : "";
						if (metaContent.IndexOf("charset", StringComparison.Ordinal) != -1)
						{
							metaContent = metaContent.Substring(metaContent.IndexOf("charset", StringComparison.Ordinal), metaContent.Length - metaContent.IndexOf("charset", StringComparison.Ordinal));
							charset = metaContent.Split('=')[1];
							break;
						}
						// 2.2、html5 <meta charset="UTF-8" />
						if (!string.IsNullOrEmpty(metaCharset))
						{
							charset = metaCharset;
							break;
						}
					}
				}
			}

			// 3、todo use tools as cpdetector for content decode
			try
			{
				return Encoding.GetEncoding(string.IsNullOrEmpty(charset) ? "UTF-8" : charset);
			}
			catch
			{
				return Encoding.UTF8;
			}
		}
	}

	internal class MyHttpMessageHandler
	{

	}

	//	public static class HttpClientExtensions
	//	{
	//#if NETNative
	//        internal static readonly Version DefaultRequestVersion = HttpVersion.Version20;
	//#else
	//		internal static readonly Version DefaultRequestVersion = HttpVersion.Version11;
	//#endif
	//		internal static readonly Version DefaultResponseVersion = HttpVersion.Version11;

	//		internal static bool IsHttpUri(Uri uri)
	//		{
	//			Debug.Assert(uri != null);

	//			string scheme = uri.Scheme;
	//			return string.Equals("http", scheme, StringComparison.OrdinalIgnoreCase) ||
	//				string.Equals("https", scheme, StringComparison.OrdinalIgnoreCase);
	//		}

	//		// Returns true if the task was faulted or canceled and sets tcs accordingly.
	//		internal static bool HandleFaultsAndCancelation<T>(Task task, TaskCompletionSource<T> tcs)
	//		{
	//			Debug.Assert(task.IsCompleted); // Success, faulted, or cancelled
	//			if (task.IsFaulted)
	//			{
	//				tcs.TrySetException(task.Exception.GetBaseException());
	//				return true;
	//			}
	//			else if (task.IsCanceled)
	//			{
	//				tcs.TrySetCanceled();
	//				return true;
	//			}
	//			return false;
	//		}

	//		// Always specify TaskScheduler.Default to prevent us from using a user defined TaskScheduler.Current.
	//		// 
	//		// Since we're not doing any CPU and/or I/O intensive operations, continue on the same thread.
	//		// This results in better performance since the continuation task doesn't get scheduled by the
	//		// scheduler and there are no context switches required.
	//		internal static Task ContinueWithStandard(this Task task, Action<Task> continuation)
	//		{
	//			return task.ContinueWith(continuation, CancellationToken.None,
	//				TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	//		}

	//		internal static Task ContinueWithStandard(this Task task, object state, Action<Task, object> continuation)
	//		{
	//			return task.ContinueWith(continuation, state, CancellationToken.None,
	//				TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	//		}

	//		internal static Task ContinueWithStandard<T>(this Task<T> task, Action<Task<T>> continuation)
	//		{
	//			return task.ContinueWith(continuation, CancellationToken.None,
	//				TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	//		}

	//		internal static Task ContinueWithStandard<T>(this Task<T> task, object state, Action<Task<T>, object> continuation)
	//		{
	//			return task.ContinueWith(continuation, state, CancellationToken.None,
	//				TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	//		}

	//		private static bool HandleRequestFaultsAndCancelation<T>(Task<HttpResponseMessage> task,
	//	TaskCompletionSource<T> tcs)
	//		{
	//			if (HandleFaultsAndCancelation(task, tcs))
	//			{
	//				return true;
	//			}

	//			HttpResponseMessage response = task.Result;
	//			if (!response.IsSuccessStatusCode)
	//			{
	//				if (response.Content != null)
	//				{
	//					response.Content.Dispose();
	//				}

	//				tcs.TrySetException(new HttpRequestException(
	//					string.Format(System.Globalization.CultureInfo.InvariantCulture,
	//						"Response status code does not indicate success: {0} ({1}).", (int)response.StatusCode,
	//						response.ReasonPhrase)));
	//				return true;
	//			}
	//			return false;
	//		}

	//		public static Task<T> GetContentAsync<T>(this HttpClient client, Uri requestUri, HttpCompletionOption completionOption, T defaultValue,
	// Func<HttpContent, Task<T>> readAs)
	//		{
	//			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

	//			client.GetAsync(requestUri, completionOption).ContinueWithStandard(requestTask =>
	//			{
	//				if (HandleRequestFaultsAndCancelation(requestTask, tcs))
	//				{
	//					return;
	//				}
	//				HttpResponseMessage response = requestTask.Result;
	//				if (response.Content == null)
	//				{
	//					tcs.TrySetResult(defaultValue);
	//					return;
	//				}

	//				try
	//				{
	//					readAs(response.Content).ContinueWithStandard(contentTask =>
	//					{
	//						if (!HandleFaultsAndCancelation(contentTask, tcs))
	//						{
	//							tcs.TrySetResult(contentTask.Result);
	//						}
	//					});
	//				}
	//				catch (Exception ex)
	//				{
	//					tcs.TrySetException(ex);
	//				}
	//			});

	//			return tcs.Task;
	//		}
	//	}
}
