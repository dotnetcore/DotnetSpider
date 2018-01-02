#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 页面解析器、抽取器的抽象
	/// </summary>
	public abstract class BasePageProcessor : IPageProcessor
	{
		/// <summary>
		/// 目标链接的解析器、抽取器
		/// </summary>
		public ITargetUrlsExtractor TargetUrlsExtractor { get; set; }

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected abstract void Handle(Page page);

		/// <summary>
		/// 解析数据结果, 解析目标链接
		/// </summary>
		/// <param name="page">页面数据</param>
		public void Process(Page page, ISpider spider)
		{
			if (TargetUrlsExtractor != null)
			{
				bool isTarget = true;
				if ((page.Request.Depth != 1 || Env.ProcessorFilterDefaultRequest) && TargetUrlsExtractor.TargetUrlPatterns != null && TargetUrlsExtractor.TargetUrlPatterns.Count > 0 && !TargetUrlsExtractor.TargetUrlPatterns.Contains(null))
				{
					foreach (var regex in TargetUrlsExtractor.TargetUrlPatterns)
					{
						isTarget = regex.IsMatch(page.Url);
						if (isTarget)
						{
							break;
						}
					}
				}

				if (!isTarget)
				{
					return;
				}
			}

			Handle(page);

			// IAfterDownloaderHandler中可以实现解析, 有可能不再需要解析了
			if (!page.SkipExtractTargetUrls && TargetUrlsExtractor != null)
			{
				ExtractUrls(page, spider);
			}
		}

		/// <summary>
		/// 解析目标链接并添加到Page对象中, 供Spider对象添加到对列中
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="spider">爬虫对象</param>
		protected virtual void ExtractUrls(Page page, ISpider spider)
		{
			var links = TargetUrlsExtractor.ExtractRequests(page, spider.Site);
			if (links != null)
			{
				foreach (var link in links)
				{
					page.AddTargetRequest(link);
				}
			}
		}
	}
}
