using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using DotnetSpider.Extension.Model.Formatter;
using Newtonsoft.Json.Linq;
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
		public TargetUrlsHandler TargetUrlsHandler;
		public DataHandler DataHandler;
		private List<TargetUrlExtractor> TargetUrlExtractors { get; } = new List<TargetUrlExtractor>();

		private readonly EntitySpider _spiderContext;

		public EntityProcessor(EntitySpider spiderContext)
		{
			Site = spiderContext.Site;
			_spiderContext = spiderContext;
		}

		public void AddTargetUrlExtractor(TargetUrlExtractor targetUrlExtractor)
		{
			TargetUrlExtractors.Add(targetUrlExtractor);
		}

		public void AddEntity(EntityMetadata entityDefine)
		{
			EntityExtractorList.Add(GenerateExtractor(entityDefine));
		}

		private IEntityExtractor GenerateExtractor(EntityMetadata entityDefine)
		{
			return new EntityExtractor(entityDefine.Entity.Name, _spiderContext.GlobalValues, entityDefine);
		}

		public void Process(Page page)
		{
			foreach (IEntityExtractor pageModelExtractor in EntityExtractorList)
			{
				List<JObject> list = pageModelExtractor.Process(page);

				if (list == null || list.Count == 0)
				{
					continue;
				}

				if (DataHandler != null)
				{
					foreach (var data in list)
					{
						switch (DataHandler.Handle(data))
						{
							case DataHandler.ResultType.MissTargetUrls:
								{
									page.MissTargetUrls = true;
									break;
								}
							case DataHandler.ResultType.Ok:
								{
									break;
								}
						}
					}
				}
				page.AddResultItem(pageModelExtractor.EntityName, list);
			}

			if (!page.MissTargetUrls)
			{
				if (TargetUrlsHandler == null)
				{
					ExtractLinks(page, TargetUrlExtractors);
				}
				else
				{
					page.AddTargetRequests(TargetUrlsHandler.Handle(page));
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
		private void ExtractLinks(Page page, List<TargetUrlExtractor> targetUrlExtractInfos)
		{
			if (targetUrlExtractInfos == null)
			{
				return;
			}

			foreach (var targetUrlExtractInfo in targetUrlExtractInfos)
			{
				var urlRegionSelector = targetUrlExtractInfo.RegionSelector;
				var formatters = targetUrlExtractInfo.Formatters;
				var urlPatterns = targetUrlExtractInfo.Regexes;

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
							page.AddTargetRequest(new Request(link, page.Request.NextDepth, page.Request.Extras));
						}
					}
				}
			}
		}

		public Site Site { get; set; }
	}
}
