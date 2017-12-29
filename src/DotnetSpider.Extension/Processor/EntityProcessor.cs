using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using System.Linq;
using System.Text.RegularExpressions;

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

		public EntityProcessor(DataHandler<T> dataHandler = null, string tableName = null)
		{
			Extractor = new EntityExtractor<T>(dataHandler, tableName);

			RegionAndPatternTargetUrlsExtractor regionAndPatternTargetUrlsExtractor;
			if (TargetUrlsExtractor == null)
			{
				regionAndPatternTargetUrlsExtractor = new RegionAndPatternTargetUrlsExtractor();
				TargetUrlsExtractor = regionAndPatternTargetUrlsExtractor;
			}
			else
			{
				regionAndPatternTargetUrlsExtractor = TargetUrlsExtractor as RegionAndPatternTargetUrlsExtractor;
			}
			if (regionAndPatternTargetUrlsExtractor == null)
			{
				return;
			}
			if (Extractor.EntityDefine.TargetUrlsSelectors != null && Extractor.EntityDefine.TargetUrlsSelectors.Count > 0)
			{
				foreach (var targetUrlsSelector in Extractor.EntityDefine.TargetUrlsSelectors)
				{
					var patterns = targetUrlsSelector.Patterns?.Select(x => x?.Trim()).Distinct().ToArray();
					var xpaths = targetUrlsSelector.XPaths?.Select(x => x?.Trim()).Distinct().ToList();
					if (xpaths == null && patterns == null)
					{
						throw new SpiderException("Region xpath and patterns should not be null both.");
					}
					if (xpaths != null && xpaths.Count > 0)
					{
						foreach (var xpath in xpaths)
						{
							regionAndPatternTargetUrlsExtractor.AddTargetUrlExtractor(xpath, patterns);
						}
					}
					else
					{
						if (patterns != null && patterns.Length > 0)
						{
							regionAndPatternTargetUrlsExtractor.AddTargetUrlExtractor(null, patterns);
						}
					}
				}
			}
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal virtual bool ContainsTargetUrlRegion(string region)
		{
			return (TargetUrlsExtractor as RegionAndPatternTargetUrlsExtractor).ContainsTargetUrlRegion(region);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal virtual List<Regex> GetTargetUrlPatterns(string region)
		{
			return (TargetUrlsExtractor as RegionAndPatternTargetUrlsExtractor).GetTargetUrlPatterns(region);
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
