using System;
using System.Collections.Generic;
using System.Net;
using DotnetSpider.Core.Selector;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 用于存储下载的内容, 解析到的目标链接等信息
	/// </summary>
	public class Page
	{
		private readonly object _locker = new object();
		private Selectable _selectable;
		private string _content;

		/// <summary>
		/// 下载的内容类型, 自动识别
		/// </summary>
		public ContentType ContentType { get; internal set; } = ContentType.Html;

		/// <summary>
		/// 当前页面的链接
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// 最终请求的链接, 当发生30X跳转时与Url的值不一致
		/// </summary>
		public string TargetUrl { get; set; }

		/// <summary>
		/// 页面的Http请求
		/// </summary>
		public Request Request { get; }

		/// <summary>
		/// 是否需要重试当前页面
		/// </summary>
		public bool Retry { get; set; }

		/// <summary>
		/// 对此页面跳过解析目标链接的操作
		/// </summary>
		public bool SkipExtractTargetUrls { get; set; }

		/// <summary>
		/// 当前页面解析出来的目标链接不加入到调度队列中
		/// </summary>
		public bool SkipTargetUrls { get; set; }

		/// <summary>
		/// 当前页面解析的数据不传入数据管道中作处理
		/// </summary>
		public bool Skip { get; set; }

		public ResultItems ResultItems { get; } = new ResultItems();

		public HttpStatusCode? StatusCode => Request?.StatusCode;

		public string Padding { get; set; }

		public string Content
		{
			get => _content;
			set
			{
				if (!Equals(value, _content))
				{
					_content = value;
					_selectable = null;
				}
			}
		}

		public Exception Exception { get; set; }

		public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

		/// <summary>
		/// Whether remove outbound urls.
		/// </summary>
		public bool RemoveOutboundLinks { get; }

		/// <summary>
		/// Only used to remove outbound urls.
		/// </summary>
		public string[] Domains { get; }

		/// <summary>
		/// Get selectable interface
		/// </summary>
		/// <returns></returns>
		public Selectable Selectable
		{
			get
			{
				if (_selectable == null)
				{
					string urlPadding = ContentType == ContentType.Json ? Padding : Request.Url.ToString();
					_selectable = new Selectable(Content, urlPadding, ContentType, RemoveOutboundLinks ? Domains : null);
				}
				return _selectable;
			}
		}

		public Page(Request request, params string[] domains)
		{
			Request = request;
			Url = request.Url.ToString();
			ResultItems.Request = request;
			RemoveOutboundLinks = domains != null && domains.Length > 0;
			Domains = domains;
		}

		/// <summary>
		/// Store extract results
		/// </summary>
		/// <param name="key"></param>
		/// <param name="field"></param>
		public void AddResultItem(string key, dynamic field)
		{
			ResultItems.AddOrUpdateResultItem(key, field);
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<string> requests)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					var request = new Request(s1, Request.Extras) { Depth = Request.NextDepth };
					if (request.IsAvailable)
					{
						TargetRequests.Add(request);
					}
				}
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<Request> requests)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (var r in requests)
				{
					r.Depth = Request.NextDepth;
					TargetRequests.Add(r);
				}
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="priority"></param>
		public void AddTargetRequests(IList<string> requests, int priority)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					Request request = new Request(s1, Request.Extras) { Priority = priority, Depth = Request.NextDepth };
					if (request.IsAvailable)
					{
						TargetRequests.Add(request);
					}
				}
			}
		}

		/// <summary>
		/// Add url to fetch
		/// </summary>
		/// <param name="requestString"></param>
		/// <param name="increaseDeep"></param>
		public void AddTargetRequest(string requestString, bool increaseDeep = true)
		{
			lock (_locker)
			{
				if (string.IsNullOrEmpty(requestString) || requestString.Equals("#"))
				{
					return;
				}

				requestString = UrlUtils.CanonicalizeUrl(requestString, Url);
				var request = new Request(requestString, Request.Extras)
				{
					Depth = Request.NextDepth
				};

				if (increaseDeep)
				{
					request.Depth = Request.NextDepth;
				}
				TargetRequests.Add(request);
			}
		}

		/// <summary>
		/// Add requests to fetch
		/// </summary>		 
		public void AddTargetRequest(Request request, bool increaseDeep = true)
		{
			if (request == null)
			{
				return;
			}
			lock (_locker)
			{
				if (request.IsAvailable)
				{
					if (increaseDeep)
					{
						request.Depth = Request.NextDepth;
					}
					TargetRequests.Add(request);
				}
			}
		}
	}
}
