using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using System.Linq;
using System.Text.RegularExpressions;
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
		/// <param name="targetRequestExtractor">目标链接的解析器</param>
		/// <param name="dataHandlers">数据处理器</param>
		public ModelProcessor(IModel model, IModelExtractor extractor = null, params IDataHandler[] dataHandlers)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));

			Extractor = extractor ?? new ModelExtractor();

			var patterns = new HashSet<string>();
			foreach (var ps in model.Targets.Select(t => t.Patterns))
			{
				if (ps != null)
				{
					foreach (var p in ps)
					{
						patterns.Add(p);
					}
				}
			}
			var excludePatterns = new HashSet<string>();
			foreach (var ps in model.Targets.Select(t => t.ExcludePatterns))
			{
				if (ps != null)
				{
					foreach (var p in ps)
					{
						excludePatterns.Add(p);
					}
				}
			}
			Filter = new PatternFilter(patterns, excludePatterns);

			var xpaths = new HashSet<string>();
			foreach (var xs in model.Targets.Select(t => t.XPaths))
			{
				if (xs != null)
				{
					foreach (var x in xs)
					{
						xpaths.Add(x);
					}
				}
			}
			if (xpaths.Any(x => x == null || x == "."))
			{
				RequestExtractor = new XPathRequestExtractor(".");
			}
			else
			{
				foreach (var xpath in xpaths)
				{
					RequestExtractor = new XPathRequestExtractor(xpaths);
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

			page.AddResultItem(Model.Identity, items);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal bool ContainsXpath(string xpath)
		{
			return ((XPathRequestExtractor)RequestExtractor).ContainsXpath(xpath);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal bool ContainsPattern(string pattern)
		{
			return ((PatternFilter)Filter).ContainsPattern(pattern);
		}
	}
}