#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Core.Processor
{
	public abstract class BasePageProcessor : IPageProcessor
	{
		public ITargetUrlsExtractor TargetUrlsExtractor { get; set; }

		protected abstract void Handle(Page page);

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

		protected virtual void ExtractUrls(Page page, ISpider spider)
		{
			var links = TargetUrlsExtractor.ExtractUrls(page, spider.Site);
			if (links != null)
			{
				foreach (var link in links)
				{
					var request = new Request(link, page.Request.Extras);
					page.AddTargetRequest(request);
				}
			}
		}
	}
}
