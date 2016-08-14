using System;
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
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : IPageProcessor
	{
		internal class TargetUrlExtractorInfo
		{
			public List<Regex> Patterns { get; set; } = new List<Regex>();
			public List<Formatter> Formatters { get; set; }
			public ISelector Region { get; set; }
            public Dictionary<string, dynamic> Extras { get; set; } = new Dictionary<string, dynamic>();
		}

		protected readonly IList<IEntityExtractor> EntityExtractorList = new List<IEntityExtractor>();
		public TargetUrlsHandler TargetUrlsHandler;
		public DataHandler DataHandler;
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
					SelectorUtil.ExtractLinks(null, page, TargetUrlExtractors);
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

		public Site Site { get; set; }
	}
}
