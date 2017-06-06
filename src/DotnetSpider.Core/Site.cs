using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core.Proxy;
using System.Net;
using DotnetSpider.Core.Downloader;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object contains setting for crawler.
	/// </summary>
	public class Site
	{
		public IHttpProxyPool HttpProxyPool { get; set; }
		private Encoding _encoding = Encoding.UTF8;
		private string _encodingName;

		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, string> Arguments = new Dictionary<string, string>();
		public ContentType ContentType { get; set; } = ContentType.Html;
		public bool RemoveOutboundLinks { get; set; }
		/// <summary>
		/// 采集目标域名的正则
		/// </summary>
		public string[] Domains { get; set; }
		public Cookies Cookies { get; set; } = new Cookies();

		public Site()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}

		/// <summary>
		/// User agent
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// User agent
		/// </summary>
		public string Accept { get; set; }

		/// <summary>
		/// Whether download pictiures
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

		public List<Request> StartRequests { get; set; } = new List<Request>();

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

		public void ClearStartRequests()
		{
			lock (this)
			{
				StartRequests.Clear();
				GC.Collect();
			}
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

		public override string ToString()
		{
			return "Site{" +
					", userAgent='" + UserAgent + '\'' +
					", cookies=" + Cookies +
					", charset='" + Encoding + '\'' +
					", startRequests=" + StartRequests +
					", cycleRetryTimes=" + CycleRetryTimes +
					", timeOut=" + Timeout +
					", acceptStatCode=" + AcceptStatCode +
					", headers=" + Headers +
					'}';
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
			set => Cookies.StringPart = value;
		}

		public void SetCookies(Dictionary<string, string> cookies)
		{
			Cookies.PairPart = cookies;
		}
	}
}