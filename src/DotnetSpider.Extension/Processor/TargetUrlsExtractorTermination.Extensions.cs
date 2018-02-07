using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// 翻页目标链接的中止器
	/// </summary>
	public class PaginationTermination : ITargetUrlsExtractorTermination
	{
		/// <summary>
		/// 取得总页数的元素选择器
		/// </summary>
		public SelectorAttribute TotalPageSelector { get; set; }

		/// <summary>
		/// 对总页数的格式化
		/// </summary>
		public Formatter[] TotalPageFormatters { get; set; }

		/// <summary>
		/// 取得当前页的元素选择器
		/// </summary>
		public SelectorAttribute CurrenctPageSelector { get; set; }

		/// <summary>
		/// 对当前页的格式化
		/// </summary>
		public Formatter[] CurrnetPageFormatters { get; set; }

		/// <summary>
		/// Return true, skip all urls from target urls extractor.
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>是否到了最终一个链接</returns>
		public bool IsTermination(Page page)
		{
			if (TotalPageSelector == null || CurrenctPageSelector == null)
			{
				throw new SpiderException("Total page selector or current page selector should not be null");
			}
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}
			var totalStr = GetSelectorValue(page, TotalPageSelector);
			var currentStr = GetSelectorValue(page, CurrenctPageSelector);

			return currentStr == totalStr;
		}

		private string GetSelectorValue(Page page, SelectorAttribute selectorAttribute)
		{
			string result = string.Empty;
			var selector = selectorAttribute.ToSelector();
			if (selectorAttribute.Type == SelectorType.Enviroment)
			{
				if (selector is EnviromentSelector enviromentSelector)
				{
					result = SelectorUtil.GetEnviromentValue(enviromentSelector.Field, page, 0)?.ToString();
				}
			}
			else
			{
				result = page.Selectable.Select(selector).GetValue();
			}

			if (!string.IsNullOrEmpty(result) && TotalPageFormatters != null)
			{
				foreach (var formatter in TotalPageFormatters)
				{
					result = formatter.Formate(result)?.ToString();
				}
			}

			if (string.IsNullOrWhiteSpace(result))
			{
				throw new SpiderException("The result of total selector is null");
			}
			else
			{
				return result;
			}
		}
	}
}
