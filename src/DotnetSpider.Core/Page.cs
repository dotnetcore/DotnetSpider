using System;
using System.Collections.Generic;
using System.Net;
using DotnetSpider.Core.Selector;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core
{
    /// <summary>
    /// Object storing extracted result and urls to fetch. 
    /// 存储提取结果和URL。
    /// </summary>
    public class Page
	{
		private readonly object _locker = new object();
		private Selectable _selectable;
		private string _content;

		public ContentType ContentType { get; internal set; } = ContentType.Html;

		/// <summary>
		/// Url of current page
		/// </summary>
		/// <returns></returns>
		public string Url { get; }

        /// <summary>
        /// 最终请求的链接, 当发生30X跳转时与Url的值不一致
        /// </summary>
        public string TargetUrl { get; set; }

		/// <summary>
		/// Title of current page.
		/// </summary>
		public string Title { get; set; }


        /// <summary>
        /// 页面的Http请求
        /// </summary>
        public Request Request { get; }

        /// <summary>
        /// 是否需要重试当前页面
        /// </summary>
        public bool Retry { get; set; }

        /// <summary>
        /// 对此页面跳过解析目标链接
        /// </summary>
        public bool SkipExtractTargetUrls { get; set; }

        /// <summary>
        /// 页面解析出来的目标链接不加入到调度队列中
        /// </summary>
        public bool SkipTargetUrls { get; set; }

        /// <summary>
        /// 页面解析的数据不传入数据管道中作处理
        /// </summary>
        public bool Skip { get; set; }
        /// <summary>
        /// 页面解析的数据结果
        /// </summary>
        public ResultItems ResultItems { get; } = new ResultItems();

		public HttpStatusCode? StatusCode => Request?.StatusCode;

		public string Padding { get; set; }
        /// <summary>
        /// 下载到的文本内容
        /// </summary>
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
        /// <summary>
        /// 下载页面内容时截获的异常
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// 页面解析到的目标链接
        /// </summary>
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
        /// 查询器
        /// </summary>
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


        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="request">请求信息</param>
        public Page(Request request)
        {
            Request = request;
            Url = request.Url.ToString();
            ResultItems.Request = request;
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
        /// 添加解析到的数据结果
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="field">数据结果</param>
        public void AddResultItem(string key, dynamic field)
		{
			ResultItems.AddOrUpdateResultItem(key, field);
		}

        /// <summary>
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="urls">链接</param>
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
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="requests">链接</param>
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
        /// 添加解析到的目标链接, 添加到队列中
        /// </summary>
        /// <param name="request">链接</param>
        /// <param name="increaseDeep">目标链接的深度是否升高</param>
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
