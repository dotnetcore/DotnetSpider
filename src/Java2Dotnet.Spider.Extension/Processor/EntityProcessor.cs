using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Extension.Model;
using Newtonsoft.Json.Linq;
using Site = Java2Dotnet.Spider.Core.Site;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace Java2Dotnet.Spider.Extension.Processor
{
	public class EntityProcessor : IPageProcessor
	{
		protected readonly IList<IEntityExtractor> EntityExtractorList = new List<IEntityExtractor>();
		public Func<Page, IList<Request>> GetCustomizeTargetUrls;
		private readonly SpiderContext _spiderContext;

		public EntityProcessor(SpiderContext spiderContext)
		{
			Site = spiderContext.Site;
			_spiderContext = spiderContext;
		}

		public void AddEntity(JObject entityDefine)
		{
			EntityExtractorList.Add(GenerateExtractor(entityDefine));
		}

		private IEntityExtractor GenerateExtractor(JObject entityDefine)
		{
			return new EntityExtractor(entityDefine.SelectToken("$.Identity").ToString(), _spiderContext.EnviromentValues, entityDefine);
		}

		public void Process(Page page)
		{
			foreach (IEntityExtractor pageModelExtractor in EntityExtractorList)
			{
				dynamic process = pageModelExtractor.Process(page);
				if (process == null || (process is IEnumerable && !((IEnumerable)process).GetEnumerator().MoveNext()))
				{
					continue;
				}

				page.AddResultItem(pageModelExtractor.EntityName, process);

				if (!page.MissTargetUrls)
				{
					if (GetCustomizeTargetUrls == null)
					{
						ExtractLinks(page, pageModelExtractor.TargetUrlExtractInfos);
					}
					else
					{
						page.AddTargetRequests(GetCustomizeTargetUrls(page));
					}
				}
			}

			if (page.ResultItems.Results.Count == 0)
			{
				page.ResultItems.IsSkip = true;
			}
		}

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		/// <param name="targetUrlExtractInfos"></param>
		private void ExtractLinks(Page page, List<TargetUrlExtractInfo> targetUrlExtractInfos)
		{
			foreach (var targetUrlExtractInfo in targetUrlExtractInfos)
			{
				var urlRegionSelector = targetUrlExtractInfo.TargetUrlRegionSelector;
				var formatter = targetUrlExtractInfo.TargetUrlFormatter;
				var urlPatterns = targetUrlExtractInfo.Patterns;

				var links = urlRegionSelector == null ? page.Selectable.Links().Value : (page.Selectable.SelectList(urlRegionSelector)).Links().Value;
				if (links == null)
				{
					return;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				if (formatter != null)
				{
					List<string> tmp = new List<string>();
					foreach (var link in links)
					{
						tmp.Add(formatter.Formate(link));
					}
					links = tmp;
				}

				List<string> tmpLinks = new List<string>();
				foreach (var link in links)
				{
#if !NET_CORE
					tmpLinks.Add(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(link)));
#else
					tmpLinks.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
#endif
				}
				links = tmpLinks;

				if (urlPatterns == null || urlPatterns.Count == 0)
				{
					page.AddTargetRequests(links);
					return;
				}

				foreach (Regex targetUrlPattern in urlPatterns)
				{
					foreach (string link in links)
					{
						if (targetUrlPattern.IsMatch(link))
						{
							page.AddTargetRequest(new Request(link, page.Request.NextDepth, page.Request.Extras)
							{
							});
						}
					}
				}
			}
		}

		public Site Site { get; }
	}
}
