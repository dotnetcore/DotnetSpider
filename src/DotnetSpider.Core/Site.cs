using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core.Proxy;
using System.Net;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object contains setting for crawler.
	/// </summary>
	public class Site
	{
		private ProxyPool _httpProxyPool = new ProxyPool();
		private string _domain;
		private Encoding _encoding = Encoding.UTF8;
		private string _encodingName;

		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		public ContentType ContentType { get; set; } = ContentType.Html;

		public Dictionary<string, string> Arguments = new Dictionary<string, string>();

		/// <summary>
		/// User agent
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// User agent
		/// </summary>
		public string Accept { get; set; }

		/// <summary>
		/// Set the domain of site.
		/// </summary>
		/// <returns></returns>
		public string Domain
		{
			get
			{
				if (_domain == null && StartRequests != null && StartRequests.Count > 0)
				{
					_domain = StartRequests[0].Url.Host;
				}
				return _domain;
			}
			set
			{
				_domain = value;
			}
		}

		/// <summary>
		/// Set charset of page manually. 
		/// When charset is not set or set to null, it can be auto detected by Http header.
		/// </summary>
		public string EncodingName
		{
			get
			{
				return _encodingName;
			}
			set
			{
				if (_encodingName != value)
				{
					_encodingName = value;
					_encoding = Encoding.GetEncoding(_encodingName);
				}
			}
		}

		internal Encoding Encoding => _encoding;

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
		public int MaxSleepTime { get; set; } = 10;

		public int MinSleepTime { get; set; } = 1;

		/// <summary>
		/// Get or Set retry times immediately when download fail, 5 by default.
		/// </summary>
		/// <returns></returns>
		public int RetryTimes { get; set; } = 5;

		/// <summary>
		/// When cycleRetryTimes is more than 0, it will add back to scheduler and try download again. 
		/// </summary>
		public int CycleRetryTimes { get; set; } = 5;

		public string Cookie { get; set; }
        /// <summary>
        /// Same content as Cookie, but this collection version can be used by webdrivers more handily.
        /// </summary>
        public List<OpenQA.Selenium.Cookie> Cookies { get; set; } = new List<OpenQA.Selenium.Cookie>();

        /// <summary>
        /// Set or Get up httpProxy for this site
        /// </summary>
        public string HttpProxy { get; set; }

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
			return AddStartRequest(new Request(startUrl, 1, null));
		}

		/// <summary>
		/// Add a url to start request. 
		/// </summary>
		/// <param name="startUrl"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public Site AddStartUrl(string startUrl, IDictionary<string, object> data)
		{
			return AddStartRequest(new Request(startUrl, 1, data));
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
				if (Domain == null)
				{
					Domain = startRequest.Url.Host;
				}
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
					"domain='" + Domain + '\'' +
					", userAgent='" + UserAgent + '\'' +
					", cookies=" + Cookie +
					", charset='" + Encoding + '\'' +
					", startRequests=" + StartRequests +
					", retryTimes=" + RetryTimes +
					", cycleRetryTimes=" + CycleRetryTimes +
					", timeOut=" + Timeout +
					", acceptStatCode=" + AcceptStatCode +
					", headers=" + Headers +
					'}';
		}

		/// <summary>
		/// add http proxy , string[0]:ip, string[1]:port
		/// </summary>
		/// <param name="httpProxyList"></param>
		/// <returns></returns>
		public Site AddHttpProxies(List<string[]> httpProxyList)
		{
			_httpProxyPool = new ProxyPool(httpProxyList);
			return this;
		}

		public bool HttpProxyPoolEnable => _httpProxyPool.Enable;

		public HttpHost GetHttpProxyFromPool()
		{
			return _httpProxyPool.GetProxy();
		}

		public void ReturnHttpProxyToPool(HttpHost proxy, int statusCode)
		{
			_httpProxyPool.ReturnProxy(proxy, statusCode);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reuseInterval">Re use interval time</param>
		/// <returns></returns>
		public Site SetProxyReuseInterval(int reuseInterval)
		{
			_httpProxyPool.SetReuseInterval(reuseInterval);
			return this;
		}
	}
}