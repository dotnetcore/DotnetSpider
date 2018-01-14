using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// 针对爬虫实体类的页面解析器、抽取器
	/// </summary>
	public class EntityProcessor<T> : BasePageProcessor, IEntityProcessor where T : ISpiderEntity
	{
		/// <summary>
		/// 针对爬虫实体类的页面解析器、抽取器
		/// </summary>
		public IEntityExtractor<T> Extractor { get; }
		
		/// <summary>
		/// 爬虫实体类的定义
		/// </summary>
		public IEntityDefine EntityDefine => Extractor?.EntityDefine;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public EntityProcessor(ITargetUrlsExtractor targetUrlsExtractor, IDataHandler<T> dataHandler, string tableName)
		{
			if (targetUrlsExtractor != null)
			{
				TargetUrlsExtractor = targetUrlsExtractor;
			}

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
		/// 构造方法
		/// </summary>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public EntityProcessor(IDataHandler<T> dataHandler) : this(null, dataHandler, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		public EntityProcessor(ITargetUrlsExtractor targetUrlsExtractor) : this(targetUrlsExtractor, null, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public EntityProcessor(IDataHandler<T> dataHandler, string tableName) : this(null, dataHandler, tableName)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="tableName"></param>
		public EntityProcessor(string tableName) : this(null, null, tableName)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		public EntityProcessor() : this(null, null, null)
		{
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

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
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
