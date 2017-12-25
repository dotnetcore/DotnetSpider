using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core.Proxy;
using System.Net;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object contains setting for spider.
	/// </summary>
	public class Site
	{
		private Encoding _encoding = Encoding.UTF8;
		private string _encodingName;

		/// <summary>
		/// 代理池
		/// </summary>
		public IHttpProxyPool HttpProxyPool { get; set; }

		/// <summary>
		/// 设置全局Http头
		/// </summary>
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// 配置采集的是Json还是Html, 如果是Auto则会自动检测下载的内容。
		/// </summary>
		public ContentType ContentType { get; set; } = ContentType.Auto;

		/// <summary>
		/// 是否去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; }

		/// <summary>
		/// 采集目标的Domain, 如果RemoveOutboundLinks为True, 则Domain不同的链接会被排除
		/// </summary>
		public string[] Domains { get; set; }

		/// <summary>
		/// 设置全局Cookie
		/// </summary>
		public Cookies Cookies { get; set; }

		/// <summary>
		/// 设置 User Agent
		/// </summary>
		public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

		/// <summary>
		/// 设置 User Accept
		/// </summary>
		public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

		/// <summary>
		/// Whether download pictiures or file
		/// </summary>
		public bool DownloadFiles { get; set; }

		/// <summary>
		/// Set charset of page manually. 
		/// When charset is not set or set to null, it can be auto detected by Http header.
		/// </summary>
		public string EncodingName
		{
			get => _encodingName;
			set
			{
				if (_encodingName != value)
				{
					_encodingName = value;
					_encoding = Encoding.GetEncoding(_encodingName);
				}
			}
		}

		/// <summary>
		/// 使用何种编码读取下载流。
		/// </summary>
		public Encoding Encoding => _encoding;

		/// <summary>
		/// Set or Get timeout for downloader in ms
		/// </summary>
		public int Timeout { get; set; } = 5000;

		/// <summary>
		/// Get or Set acceptStatCode. 
		/// When status code of http response is in acceptStatCodes, it will be processed. 
		/// {200} by default. 
		/// It is not necessarily to be set.
		/// </summary>
		public HashSet<HttpStatusCode> AcceptStatCode { get; set; } = new HashSet<HttpStatusCode> { HttpStatusCode.OK };

		public readonly List<Request> StartRequests = new List<Request>();

		/// <summary>
		/// Set the interval between the processing of two pages. 
		/// Time unit is micro seconds. 
		/// </summary>
		public int SleepTime { get; set; } = 100;

		/// <summary>
		/// When cycleRetryTimes is more than 0, it will add back to scheduler and try download again. 
		/// </summary>
		public int CycleRetryTimes { get; set; } = 5;

		/// <summary>
		/// Whether use gzip.  
		/// Default is true, you can set it to false to disable gzip.
		/// </summary>
		public bool IsUseGzip { get; set; }

		public Site()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}

		/// <summary>
		/// Add a url to start request. 
		/// </summary>
		/// <param name="startUrl"></param>
		/// <returns></returns>
		public Site AddStartUrl(string startUrl)
		{
			return AddStartRequest(new Request(startUrl, null));
		}

		/// <summary>
		/// Add a url to start request. 
		/// </summary>
		/// <param name="startUrl"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public Site AddStartUrl(string startUrl, IDictionary<string, object> data)
		{
			return AddStartRequest(new Request(startUrl, data));
		}

		public Site AddStartUrls(IList<string> startUrls)
		{
			foreach (var url in startUrls)
			{
				AddStartUrl(url);
			}

			return this;
		}

		public Site AddStartUrls(IDictionary<string, IDictionary<string, object>> startUrls)
		{
			foreach (var entry in startUrls)
			{
				AddStartUrl(entry.Key, entry.Value);
			}

			return this;
		}

		/// <summary>
		/// Add a request.
		/// </summary>
		/// <param name="startRequest"></param>
		/// <returns></returns>
		public Site AddStartRequest(Request startRequest)
		{
			lock (this)
			{
				StartRequests.Add(startRequest);
				return this;
			}
		}

		public void ClearStartRequests()
		{
			lock (this)
			{
				StartRequests.Clear();
			}
		}

		/// <summary>
		/// Put an Http header for downloader. 
		/// </summary>
		public Site AddHeader(string key, string value)
		{
			if (Headers.ContainsKey(key))
			{
				Headers[key] = value;
			}
			else
			{
				Headers.Add(key, value);
			}
			return this;
		}

		public UseSpecifiedUriWebProxy GetHttpProxy()
		{
			return HttpProxyPool?.GetProxy();
		}

		public void ReturnHttpProxy(UseSpecifiedUriWebProxy proxy, HttpStatusCode statusCode)
		{
			HttpProxyPool?.ReturnProxy(proxy, statusCode);
		}

		public string CookiesStringPart
		{
			set
			{
				if (Cookies == null)
				{
					Cookies = new Cookies();
				}
				Cookies.StringPart = value;
			}
			get
			{
				return Cookies == null ? string.Empty : Cookies.StringPart;
			}
		}

		public void SetCookies(Dictionary<string, string> cookies)
		{
			if (Cookies == null)
			{
				Cookies = new Cookies();
			}
			Cookies.PairPart = cookies;
		}
	}
}