using DotnetSpider.Downloader;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Formatter;

namespace DotnetSpider.Core.Processor.LastPageChecker
{
	/// <summary>
	/// 翻页目标链接的中止器
	/// </summary>
	public class PaginationLastPageChecker : ILastPageChecker
	{
		/// <summary>
		/// 取得总页数的元素选择器
		/// </summary>
		public Selector TotalPageSelector { get; set; }

		/// <summary>
		/// 对总页数的格式化
		/// </summary>
		public Formatter[] TotalPageFormatters { get; set; }

		/// <summary>
		/// 取得当前页的元素选择器
		/// </summary>
		public Selector CurrentPageSelector { get; set; }

		/// <summary>
		/// 对当前页的格式化
		/// </summary>
		public Formatter[] CurrentPageFormatters { get; set; }

		/// <summary>
		/// Return true, skip all urls from target urls extractor.
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>是否到了最终一个链接</returns>
		public bool IsLastPage(Page page)
		{
			if (TotalPageSelector == null || CurrentPageSelector == null)
			{
				throw new SpiderException("Total page selector or current page selector should not be null");
			}

			var text = page?.Content?.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			var totalStr = GetSelectorValue(page, TotalPageSelector);
			var currentStr = GetSelectorValue(page, CurrentPageSelector);

			return currentStr == totalStr;
		}

		private string GetSelectorValue(Response page, Selector selectorAttribute)
		{
			var result = string.Empty;
			var selector = selectorAttribute.ToSelector();
			if (selectorAttribute.Type == SelectorType.Enviroment)
			{
				if (selector is EnvironmentSelector environmentSelector)
				{
					result = page.Selectable().Environment(environmentSelector.Field);
				}
			}
			else
			{
				result = page.Selectable().Select(selector).GetValue();
			}

			if (!string.IsNullOrEmpty(result) && TotalPageFormatters != null)
			{
				foreach (var formatter in TotalPageFormatters)
				{
					result = formatter.Format(result)?.ToString();
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