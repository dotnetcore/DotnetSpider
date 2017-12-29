using System;
using System.Collections.Generic;
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
		private ISelectable _selectable;
		private string _content;

		/// <summary>
		/// 下载的内容类型, 自动识别
		/// </summary>
		public ContentType ContentType { get; internal set; } = ContentType.Html;

		/// <summary>
		/// 页面的链接
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

		/// <summary>
		/// 去除返回的JSON数据的最外层填补
		/// </summary>
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
		/// 查询器
		/// </summary>
		public ISelectable Selectable
		{
			get
			{
				if (_selectable == null)
				{
					string urlPadding = ContentType == ContentType.Json ? Padding : Request.Url.ToString();
					_selectable = new Selectable(Content, urlPadding, ContentType, Request.Site.RemoveOutboundLinks ? Request.Site.Domains : null);
				}
				return _selectable;
			}
		}

		public Page(Request request)
		{
			Request = request;
			Url = request.Url.ToString();
			ResultItems.Request = request;
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
		public void AddTargetRequests(IEnumerable<string> urls)
		{
			if (urls == null)
			{
				return;
			}

			foreach (string url in urls)
			{
				AddTargetRequest(url);
			}
		}

		/// <summary>
		/// 添加解析到的目标链接, 添加到队列中
		/// </summary>
		/// <param name="requests">链接</param>
		public void AddTargetRequests(IEnumerable<Request> requests)
		{
			if (requests == null)
			{
				return;
			}
			foreach (var request in requests)
			{
				AddTargetRequest(request);
			}
		}

		/// <summary>
		/// 添加解析到的目标链接, 添加到队列中
		/// </summary>
		/// <param name="urls">链接</param>
		/// <param name="priority">优先级</param>
		public void AddTargetRequests(IEnumerable<string> urls, int priority)
		{
			if (urls == null)
			{
				return;
			}
			foreach (string url in urls)
			{
				AddTargetRequest(url, priority);
			}
		}

		/// <summary>
		/// 添加解析到的目标链接, 添加到队列中
		/// </summary>
		/// <param name="url">链接</param>
		/// <param name="priority">优先级</param>
		/// <param name="increaseDeep">目标链接的深度是否升高</param>
		public void AddTargetRequest(string url, int priority = 0, bool increaseDeep = true)
		{
			if (string.IsNullOrEmpty(url) || url.Equals("#") || url.StartsWith("javascript:"))
			{
				return;
			}
			var newUrl = UrlUtils.CanonicalizeUrl(url, Url);
			var request = new Request(newUrl, Request.Extras) { Priority = priority };
			AddTargetRequest(request, increaseDeep);
		}

		/// <summary>
		/// 添加解析到的目标链接, 添加到队列中
		/// </summary>
		/// <param name="request">链接</param>
		/// <param name="increaseDeep">目标链接的深度是否升高</param>
		public void AddTargetRequest(Request request, bool increaseDeep = true)
		{
			if (request == null || !request.IsAvailable)
			{
				return;
			}
			request.Depth = increaseDeep ? Request.NextDepth : Request.Depth;
			lock (_locker)
			{
				TargetRequests.Add(request);
			}
		}
	}
}
