using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;
using Site = DotnetSpider.Core.Site;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Configuration;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : IPageProcessor
	{
		protected readonly IList<IEntityExtractor> EntityExtractorList = new List<IEntityExtractor>();
		public Func<Page, IList<Request>> GetCustomizeTargetUrls;
		public List<Model.TargetUrlExtractor> TargetUrlExtractInfos { get; set; }

		private readonly SpiderContext _spiderContext;

		public EntityProcessor(SpiderContext spiderContext)
		{
			Site = spiderContext.Site;
			_spiderContext = spiderContext;
		}

		public void AddEntity(EntityMetadata entityDefine)
		{
			EntityExtractorList.Add(GenerateExtractor(entityDefine));
		}

		private IEntityExtractor GenerateExtractor(EntityMetadata entityDefine)
		{
			return new EntityExtractor(entityDefine.Name, _spiderContext.EnviromentValues, entityDefine);
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
			}

			if (!page.MissTargetUrls)
			{
				if (GetCustomizeTargetUrls == null)
				{
					ExtractLinks(page, TargetUrlExtractInfos);
				}
				else
				{
					page.AddTargetRequests(GetCustomizeTargetUrls(page));
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
		private void ExtractLinks(Page page, List<Model.TargetUrlExtractor> targetUrlExtractInfos)
		{
			if (targetUrlExtractInfos == null)
			{
				return;
			}

			foreach (var targetUrlExtractInfo in targetUrlExtractInfos)
			{
				var urlRegionSelector = targetUrlExtractInfo.Region;
				var formatters = targetUrlExtractInfo.Formatters;
				var urlPatterns = targetUrlExtractInfo.Patterns;

				var links = urlRegionSelector == null ? page.Selectable.Links().GetValues() : (page.Selectable.SelectList(urlRegionSelector)).Links().GetValues();
				if (links == null)
				{
					return;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				if (formatters != null && formatters.Count > 0)
				{
					List<string> tmp = new List<string>();
					foreach (string link in links)
					{
						var url = new String(link.ToCharArray());
						foreach (Formatter f in formatters)
						{
							url = f.Formate(url);
						}
						tmp.Add(url);
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

		public Site Site { get; set; }
	}
}
