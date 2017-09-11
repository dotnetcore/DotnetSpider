using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using System.Linq;

namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : BasePageProcessor
	{
		private readonly IEntityExtractor _extractor;

		public EntityProcessor(Site site, EntityDefine entity)
		{
			Site = site;
			_extractor = new EntityExtractor(entity.Name, entity.SharedValues, entity);
			if (entity.TargetUrlsSelectors != null && entity.TargetUrlsSelectors.Count > 0)
			{
				foreach (var targetUrlsSelector in entity.TargetUrlsSelectors)
				{
					if (targetUrlsSelector.XPaths == null && targetUrlsSelector.Patterns == null)
					{
						throw new SpiderException("Region xpath and patterns should not be null both.");
					}
					if (targetUrlsSelector.XPaths == null)
					{
						targetUrlsSelector.XPaths = new string[] { };
					}
					foreach (var xpath in targetUrlsSelector.XPaths?.Select(x => x?.Trim()).Distinct())
					{
						AddTargetUrlExtractor(xpath, targetUrlsSelector.Patterns);
					}
				}
			}
		}

		protected override void Handle(Page page)
		{
			List<DataObject> list = _extractor.Extract(page);

			if (_extractor.DataHandler != null)
			{
				list = _extractor.DataHandler.Handle(list, page);
			}

			if (list == null || list.Count == 0)
			{
				return;
			}
			page.AddResultItem(_extractor.Name, list);
		}
	}

	public class EntityProcessor<T> : EntityProcessor where T : SpiderEntity
	{
		public EntityProcessor(Site site) : base(site, EntityDefine.Parse<T>())
		{
		}
	}
}
