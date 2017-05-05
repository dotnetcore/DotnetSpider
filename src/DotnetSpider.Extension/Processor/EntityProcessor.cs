﻿using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using Newtonsoft.Json.Linq;
using System.Linq;

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
			if (entity.TargetUrlsSelectors != null && entity.TargetUrlsSelectors.Count > 0)
			{
				var pairs = new List<string>();
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
