using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace DotnetSpider.Extension.Processor
{
	public class ModelProcessor : BasePageProcessor, IModelProcessor
	{
		private readonly List<IDataHandler> _dataHandlers = new List<IDataHandler>();

		/// <summary>
		/// 针对爬虫实体类的页面解析器、抽取器
		/// </summary>
		public IModelExtractor Extractor { get; }

		/// <summary>
		/// 爬虫实体类的定义
		/// </summary>
		public IModel Model { get; private set; }

		void AddDataHanlder(IDataHandler handler)
		{
			if (handler == null)
			{
				throw new SpiderException("Datahandler should not be null.");
			}
			_dataHandlers.Add(handler);
		}

		public ModelProcessor(IModel model) : this(model, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="extractor">爬虫实体的解析器</param>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public ModelProcessor(IModel model, IModelExtractor extractor = null, ITargetUrlsExtractor targetUrlsExtractor = null, params IDataHandler[] dataHandlers)
		{
			Model = model ?? throw new ArgumentNullException($"{nameof(model)} should not be null.");

			Extractor = extractor ?? new ModelExtractor();

			if (targetUrlsExtractor != null)
			{
				TargetUrlsExtractor = targetUrlsExtractor;
			}

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
			if (Model.TargetUrlsSelectors != null && Model.TargetUrlsSelectors.Count() > 0)
			{
				foreach (var targetUrlsSelector in Model.TargetUrlsSelectors)
				{
					var patterns = targetUrlsSelector.Patterns?.Select(x => x?.Trim()).Distinct().ToArray();
					var xpaths = targetUrlsSelector.XPaths?.Select(x => x?.Trim()).Distinct().ToList();
					if (xpaths == null && patterns == null)
					{
						throw new SpiderException("Region xpath and patterns should not be null both");
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

			if (dataHandlers != null)
			{
				foreach (var datahandler in dataHandlers)
				{
					if (datahandler != null)
					{
						_dataHandlers.Add(datahandler);
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

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			var datas = Extractor.Extract(page, Model);

			if (datas == null || datas.Count() == 0)
			{
				return;
			}

			foreach (var handler in _dataHandlers)
			{
				for (int i = 0; i < datas.Count(); ++i)
				{
					dynamic data = datas.ElementAt(i);
					handler.Handle(ref data, page);
				}
			}

			page.AddResultItem(Model.Identity, new Tuple<IModel, IEnumerable<dynamic>>(Model, datas));
		}
	}

}
