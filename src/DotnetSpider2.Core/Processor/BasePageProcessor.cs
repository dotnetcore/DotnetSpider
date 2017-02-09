using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Core.Processor
{
	public abstract class BasePageProcessor : IPageProcessor
	{
		protected abstract void Handle(Page page);

		public void Process(Page page)
		{
			if (TargetUrlPatterns != null)
			{
				bool isTarget = true;
				foreach (var regex in TargetUrlPatterns)
				{
					isTarget = regex.IsMatch(page.Url);
					if (isTarget)
					{
						break;
					}
				}
				if (!isTarget)
				{
					return;
				}
			}

			Handle(page);

			if (page.ResultItems.Results.Count == 0)
			{
				page.ResultItems.IsSkip = true;
			}
			else
			{
				page.ResultItems.IsSkip = false;
			}

			if (!page.MissExtractTargetUrls)
			{
				ExtractUrls(page);
			}
		}

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		/// <param name="targetUrlExtractInfos"></param>
		protected virtual void ExtractUrls(Page page)
		{
			VerifyTargetUrlsSelector();

			if (TargetUrlRegions == null || TargetUrlRegions.Count == 0)
			{
				return;
			}

			foreach (var urlRegionSelector in TargetUrlRegions)
			{
				var links = urlRegionSelector == null ? page.Selectable.Links().GetValues() : (page.Selectable.SelectList(urlRegionSelector)).Links().GetValues();
				if (links == null)
				{
					continue;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				List<string> tmp = new List<string>();
				foreach (string link in links)
				{
					var url = FormateUrl(link);
#if !NET_CORE
					tmp.Add(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(url)));
#else
					tmp.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(url)));
#endif
				}
				links = tmp;

				if (TargetUrlPatterns == null || TargetUrlPatterns.Count == 0)
				{
					page.AddTargetRequests(links);
					continue;
				}

				foreach (Regex targetUrlPattern in TargetUrlPatterns)
				{
					foreach (string link in links)
					{
						if (targetUrlPattern.IsMatch(link))
						{
							page.AddTargetRequest(new Request(link, page.Request.Extras));
						}
					}
				}
			}
		}

		protected virtual string FormateUrl(string url)
		{
			return url;
		}

		protected virtual void VerifyTargetUrlsSelector()
		{
			Dictionary<ISelector, List<Regex>> results = new Dictionary<ISelector, List<Regex>>();

			if ((TargetUrlRegions == null || TargetUrlRegions.Count == 0) && (TargetUrlPatterns == null || TargetUrlPatterns.Count == 0))
			{
				return;
			}
			if (TargetUrlRegions == null || TargetUrlRegions.Count == 0)
			{
				TargetUrlRegions = new HashSet<ISelector> { Selectors.XPath(".") };
			}
		}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; set; }

		public HashSet<ISelector> TargetUrlRegions { get; protected set; } = new HashSet<ISelector>();

		public HashSet<Regex> TargetUrlPatterns { get; protected set; } = new HashSet<Regex>();
	}
}
