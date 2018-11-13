using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using System.Linq;
using System;
using DotnetSpider.Core.Processor.Filter;
using DotnetSpider.Core.Processor.RequestExtractor;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extension.Model;

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
		/// <param name="dataHandlers">数据处理器</param>
		public ModelProcessor(IModel model, IModelExtractor extractor = null, params IDataHandler[] dataHandlers)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));

			Extractor = extractor ?? new ModelExtractor();

			var patterns = new HashSet<string>();
			foreach (var ps in model.Targets.Select(t => t.Patterns))
			{
				if (ps == null) continue;
				foreach (var p in ps)
				{
					patterns.Add(p);
				}
			}

			var excludePatterns = new HashSet<string>();
			foreach (var ps in model.Targets.Select(t => t.ExcludePatterns))
			{
				if (ps == null) continue;
				foreach (var p in ps)
				{
					excludePatterns.Add(p);
				}
			}

			Filter = new PatternFilter(patterns, excludePatterns);

			var xPaths = new HashSet<string>();
			foreach (var xs in model.Targets.Select(t => t.XPaths))
			{
				if (xs == null) continue;
				foreach (var x in xs)
				{
					xPaths.Add(x);
				}
			}

			RequestExtractor = xPaths.Any(x => x == null || x == ".")
				? new XPathRequestExtractor(".")
				: (xPaths.Count == 0 ? null : new XPathRequestExtractor(xPaths));

			if (dataHandlers == null) return;
			foreach (var dataHandler in dataHandlers)
			{
				if (dataHandler != null)
				{
					_dataHandlers.Add(dataHandler);
				}
			}
		}

		public void AddDataHandler(IDataHandler handler)
		{
			if (handler == null)
			{
				throw new SpiderException("DataHandler should not be null");
			}

			_dataHandlers.Add(handler);
		}

		/// <summary>
		/// 解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			var items = Extractor.Extract(page.Selectable(), Model)?.ToList();

			if (items == null || items.Count == 0)
			{
				return;
			}

			foreach (var handler in _dataHandlers)
			{
				for (var i = 0; i < items.Count; ++i)
				{
					var data = items.ElementAt(i);
					handler.Handle(ref data, page);
				}
			}

			page.AddResultItem(Model.Identity, items);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="xPath"></param>
		/// <returns></returns>
		internal bool ContainsXpath(string xPath)
		{
			return ((XPathRequestExtractor) RequestExtractor).ContainsXpath(xPath);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="pattern"></param>
		/// <returns></returns>
		internal bool ContainsPattern(string pattern)
		{
			return ((PatternFilter) Filter).ContainsPattern(pattern);
		}
	}
}