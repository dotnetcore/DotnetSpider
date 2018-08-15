using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Processor.TargetRequestExtractors;

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
		public IModel Model { get; }

		public ModelProcessor(IModel model) : this(model, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="model">数据模型</param>
		/// <param name="extractor">模型解析器</param>
		/// <param name="targetRequestExtractor">目标链接的解析器</param>
		/// <param name="dataHandlers">数据处理器</param>
		public ModelProcessor(IModel model, IModelExtractor extractor = null, ITargetRequestExtractor targetRequestExtractor = null,
			params IDataHandler[] dataHandlers)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));

			Extractor = extractor ?? new ModelExtractor();

			if (targetRequestExtractor != null)
			{
				TargetUrlsExtractor = targetRequestExtractor;
			}

			RegionAndPatternTargetRequestExtractor regionAndPatternTargetUrlsExtractor;
			if (TargetUrlsExtractor == null)
			{
				regionAndPatternTargetUrlsExtractor = new RegionAndPatternTargetRequestExtractor();
				TargetUrlsExtractor = regionAndPatternTargetUrlsExtractor;
			}
			else
			{
				regionAndPatternTargetUrlsExtractor = TargetUrlsExtractor as RegionAndPatternTargetRequestExtractor;
			}

			if (regionAndPatternTargetUrlsExtractor == null)
			{
				return;
			}

			if (Model.TargetRequestSelectors != null && Model.TargetRequestSelectors.Any())
			{
				foreach (var targetUrlsSelector in Model.TargetRequestSelectors)
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

					foreach (var p in targetUrlsSelector.ExcludePatterns)
					{
						regionAndPatternTargetUrlsExtractor.ExcludeTargetUrlPatterns.Add(new Regex(p));
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

		public void AddDataHanlder(IDataHandler handler)
		{
			if (handler == null)
			{
				throw new SpiderException("Datahandler should not be null.");
			}

			_dataHandlers.Add(handler);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal virtual bool ContainsTargetUrlRegion(string region)
		{
			return ((RegionAndPatternTargetRequestExtractor)TargetUrlsExtractor).ContainsTargetUrlRegion(region);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal virtual List<Regex> GetTargetUrlPatterns(string region)
		{
			return (TargetUrlsExtractor as RegionAndPatternTargetRequestExtractor)?.GetTargetUrlPatterns(region);
		}

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			var datas = Extractor.Extract(page.Selectable(), Model);

			if (datas == null)
			{
				return;
			}

			var items = datas.ToList();

			foreach (var handler in _dataHandlers)
			{
				for (int i = 0; i < items.Count; ++i)
				{
					dynamic data = items.ElementAt(i);
					handler.Handle(ref data, page);
				}
			}

			page.AddResultItem(Model.Identity, new Tuple<IModel, IList<dynamic>>(Model, items));
		}
	}
}