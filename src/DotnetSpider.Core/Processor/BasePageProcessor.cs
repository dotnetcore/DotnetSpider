using DotnetSpider.Downloader;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 页面解析器、抽取器的抽象
	/// </summary>
	public abstract class BasePageProcessor : IPageProcessor
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		public ILogger Logger { get; set; }

		/// <summary>
		/// 用于判断是否需要处理当前 Request, 以及解析出来的目标链接是否需要添加到队列.
		/// RequestExtractor 解析出来的结果也需验证是否符合 Filter, 如果不符合 Filter 那么最终也不会进入到 Processor, 即为无意义的 Request
		/// </summary>
		public IFilter Filter { get; set; }

		/// <summary>
		/// 解析目标链接的接口
		/// </summary>
		public IRequestExtractor RequestExtractor { get; set; }

		/// <summary>
		/// 是否最后一页的判断接口, 如果是最后一页, 则不需要执行 RequestExtractor
		/// </summary>
		public ILastPageChecker LastPageChecker { get; set; }

		/// <summary>
		/// 去掉链接#后面的所有内容
		/// </summary>
		public bool CleanPound { get; set; }

		/// <summary>
		/// 是否去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; }

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected abstract void Handle(Page page);

		/// <summary>
		/// 解析数据结果, 解析目标链接
		/// </summary>
		/// <param name="page">页面数据</param>
		public void Process(Page page)
		{
			var properties = page.Selectable(RemoveOutboundLinks).Properties;
			properties[Env.UrlPropertyKey] = page.Request.Url;
			properties[Env.TargetUrlPropertyKey] = page.TargetUrl;

			if (!(page.Request.GetProperty(Page.Depth) == 1 && !Env.FilterDefaultRequest))
			{
				if (Filter != null && !Filter.IsMatch(page.Request))
				{
					return;
				}
			}

			Handle(page);

			if (LastPageChecker != null && LastPageChecker.IsLastPage(page)) return;

			IEnumerable<Request> requests;
			if (RequestExtractor != null && (requests = RequestExtractor.Extract(page)) != null)
			{
				foreach (var link in requests)
				{
					if (Filter != null && !Filter.IsMatch(link)) continue;

					if (CleanPound)
					{
						link.Url = link.Url.Split('#')[0];
					}

					page.AddTargetRequest(link);
				}
			}
		}

		public BasePageProcessor SetRequestExtractor(IRequestExtractor requestExtractor)
		{
			RequestExtractor = requestExtractor;
			return this;
		}

		public BasePageProcessor SetFilter(IFilter filter)
		{
			Filter = filter;
			return this;
		}

		public BasePageProcessor SetLastPageChecker(ILastPageChecker lastPageChecker)
		{
			LastPageChecker = lastPageChecker;
			return this;
		}
	}
}