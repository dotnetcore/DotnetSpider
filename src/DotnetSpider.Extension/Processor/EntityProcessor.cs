using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Selector;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : BasePageProcessor
	{
		protected readonly EntityMetadata _entity;
		protected readonly IEntityExtractor _extractor;

		public EntityProcessor(Site site, EntityMetadata entity)
		{
			Site = site;
			_entity = entity;
			_extractor = new EntityExtractor(entity.Entity.Name, entity.SharedValues, entity);
			if (entity.TargetUrlExtractor != null)
			{
				if (entity.TargetUrlExtractor.Patterns != null && entity.TargetUrlExtractor.Patterns.Length > 0)
				{
					TargetUrlPatterns = new HashSet<Regex>(entity.TargetUrlExtractor.Patterns.Select(p => new Regex(p)));
				}
				if (entity.TargetUrlExtractor.XPaths != null && entity.TargetUrlExtractor.XPaths.Length > 0)
				{
					TargetUrlRegions = new HashSet<ISelector>(entity.TargetUrlExtractor.XPaths.Select(x => Selectors.XPath(x)));
				}
			}
		}

		protected override void Handle(Page page)
		{
			List<JObject> list = _extractor.Extract(page);

			if (list == null || list.Count == 0)
			{
				return;
			}

			if (_extractor.DataHandler != null)
			{
				list = _extractor.DataHandler.Handle(list, page);
			}

			page.AddResultItem(_extractor.Name, list);
		}
	}
}
