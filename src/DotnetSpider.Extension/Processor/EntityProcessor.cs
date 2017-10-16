using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using System.Linq;

namespace DotnetSpider.Extension.Processor
{
	public interface IEntityProcessor
	{
		IEntityDefine EntityDefine { get; }
	}

	public class EntityProcessor<T> : BasePageProcessor, IEntityProcessor where T : ISpiderEntity
	{
		public IEntityExtractor<T> Extractor { get; }

		public IEntityDefine EntityDefine => Extractor?.EntityDefine;

		public EntityProcessor(Site site, DataHandler<T> dataHandler = null)
		{
			Site = site;
			Extractor = new EntityExtractor<T>(dataHandler);

			if (Extractor.EntityDefine.TargetUrlsSelectors != null && Extractor.EntityDefine.TargetUrlsSelectors.Count > 0)
			{
				foreach (var targetUrlsSelector in Extractor.EntityDefine.TargetUrlsSelectors)
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
			List<T> list = Extractor.Extract(page);

			if (Extractor.DataHandler != null)
			{
				list = Extractor.DataHandler.Handle(list, page);
			}

			if (list == null || list.Count == 0)
			{
				return;
			}

			page.AddResultItem(Extractor.Name, list);
		}
	}
}
