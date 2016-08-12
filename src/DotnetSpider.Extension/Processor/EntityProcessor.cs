using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using System.Linq;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Common;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : IPageProcessor
	{
		public class TargetUrlExtractorInfo
		{
			public List<Regex> Patterns { get; set; } = new List<Regex>();
			public List<Formatter> Formatters { get; set; }
			public ISelector Region { get; set; }
            public Dictionary<string, dynamic> Extras { get; set; } = new Dictionary<string, dynamic>();
        }

        protected readonly IList<IEntityExtractor> EntityExtractorList = new List<IEntityExtractor>();
		public Func<Page, IList<Request>> GetCustomizeTargetUrls;
		private List<TargetUrlExtractorInfo> TargetUrlExtractors { get; } = new List<TargetUrlExtractorInfo>();

		private readonly EntitySpider _spiderContext;

		public EntityProcessor(EntitySpider spiderContext)
		{
			Site = spiderContext.Site;
			_spiderContext = spiderContext;
		}

		public void AddTargetUrlExtractor(TargetUrlExtractor targetUrlExtractor)
		{
			TargetUrlExtractors.Add(new TargetUrlExtractorInfo
			{
				Patterns = targetUrlExtractor.Patterns.Select(t => new Regex(t)).ToList(),
				Formatters = targetUrlExtractor.Formatters,
				Region = SelectorUtil.Parse(targetUrlExtractor.Region)
			});
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
                    SelectorUtil.ExtractLinks(null, page, TargetUrlExtractors);
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

		public Site Site { get; set; }
	}
}
