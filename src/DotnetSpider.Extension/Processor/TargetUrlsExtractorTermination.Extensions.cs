using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Processor
{
	public class PaggerTermination
	{
		public BaseSelector TotalPageSelector { get; set; }
		public Formatter[] TotalPageFormatters { get; set; }

		public BaseSelector CurrenctPageSelector { get; set; }
		public Formatter[] CurrnetPageFormatters { get; set; }

		public bool IsTermination(Page page, ITargetUrlsExtractor creator)
		{
			if (TotalPageSelector == null || CurrenctPageSelector == null)
			{
				throw new SpiderException("Total page selector or current page selector should not be null.");
			}
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}
			var totalStr = GetSelectorValue(page, TotalPageSelector);
			var currentStr = GetSelectorValue(page, CurrenctPageSelector);

			return currentStr == totalStr;
		}

		private string GetSelectorValue(Page page, BaseSelector selector)
		{
			string result = string.Empty;
			if (selector.Type == SelectorType.Enviroment)
			{
				if (SelectorUtils.Parse(selector) is EnviromentSelector enviromentSelector)
				{
					result = SelectorUtils.GetEnviromentValue(enviromentSelector.Field, page, 0)?.ToString();
				}
			}
			else
			{
				result = page.Selectable.Select(SelectorUtils.Parse(selector)).GetValue();
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
				throw new SpiderException("The result of total selector is null.");
			}
			else
			{
				return result;
			}
		}
	}
}
