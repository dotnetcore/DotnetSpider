using DotnetSpider.Common;
using DotnetSpider.Extraction;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	public class Page : Response
	{
		private readonly object _locker = new object();

		/// <summary>
		/// 是否需要重试当前页面
		/// </summary>
		public bool Retry { get; set; }

		/// <summary>
		/// 对此页面跳过解析目标链接的操作
		/// </summary>
		public bool SkipExtractedTargetRequests { get; set; }

		/// <summary>
		/// 页面解析出来的目标链接不加入到调度队列中
		/// </summary>
		public bool SkipTargetRequests { get; set; }

		/// <summary>
		/// 忽略当前页面不作解析处理
		/// </summary>
		public bool Bypass { get; set; }

		/// <summary>
		/// 页面解析的数据结果
		/// </summary>
		public ResultItems ResultItems { get; } = new ResultItems();

		/// <summary>
		/// 页面解析到的目标链接
		/// </summary>
		public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="request">请求信息</param>
		public Page(Request request)
		{
			Request = request;
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
			if (string.IsNullOrWhiteSpace(url) || url.Equals("#") || url.StartsWith("javascript:"))
			{
				return;
			}
			var newUrl = Selectable.CanonicalizeUrl(url, Request.Url);
			var request = new Request(newUrl, Request.Properties) { Priority = priority };
			AddTargetRequest(request, increaseDeep);
		}

		/// <summary>
		/// 添加解析到的目标链接, 添加到队列中
		/// </summary>
		/// <param name="request">链接</param>
		/// <param name="increaseDeep">目标链接的深度是否升高</param>
		public void AddTargetRequest(Request request, bool increaseDeep = true)
		{
			if (request == null || !IsAvailable(request))
			{
				return;
			}
			request.Depth = increaseDeep ? Request.Depth + 1 : Request.Depth;
			lock (_locker)
			{
				TargetRequests.Add(request);
			}
		}

		private bool IsAvailable(Request request)
		{
			if (string.IsNullOrWhiteSpace(request.Url))
			{
				return false;
			}

			if (request.Url.Length < 6)
			{
				return false;
			}

			var schema = request.Url.Substring(0, 5).ToLower();
			if (!schema.StartsWith("http") && !schema.StartsWith("https"))
			{
				return false;
			}

			return true;
		}
	}
}
