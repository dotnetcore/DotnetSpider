using DotnetSpider.Common;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Formatter;

namespace DotnetSpider.Core.Processor.TargetRequestExtractors
{
	/// <summary>
	/// 翻页目标链接的中止器
	/// </summary>
	public class PaginationTermination : ITargetRequestExtractorTermination
	{
		/// <summary>
		/// 取得总页数的元素选择器
		/// </summary>
		public Selector TotalPageSelector;

		/// <summary>
		/// 对总页数的格式化
		/// </summary>
		public Formatter[] TotalPageFormatters;

		/// <summary>
		/// 取得当前页的元素选择器
		/// </summary>
		public Selector CurrenctPageSelector;

		/// <summary>
		/// 对当前页的格式化
		/// </summary>
		public Formatter[] CurrnetPageFormatters { get; set; }

		/// <summary>
		/// Return true, skip all urls from target urls extractor.
		/// </summary>
		/// <param name="response">页面数据</param>
		/// <returns>是否到了最终一个链接</returns>
		public bool IsTerminated(Response response)
		{
			if (TotalPageSelector == null || CurrenctPageSelector == null)
			{
				throw new SpiderException("Total page selector or current page selector should not be null");
			}

			if (string.IsNullOrEmpty(response?.Content))
			{
				return false;
			}

			var totalStr = GetSelectorValue(response, TotalPageSelector);
			var currentStr = GetSelectorValue(response, CurrenctPageSelector);

			return currentStr == totalStr;
		}

		private string GetSelectorValue(Response page, Selector selectorAttribute)
		{
			string result = string.Empty;
			var selector = selectorAttribute.ToSelector();
			if (selectorAttribute.Type == SelectorType.Enviroment)
			{
				if (selector is EnviromentSelector enviromentSelector)
				{
					result = page.Selectable().Enviroment(enviromentSelector.Field);
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
