using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Downloader
{
	public class PaggerTermination : ITargetUrlsBuilderTermination
	{
		public BaseSelector TotalPageSelector { get; set; }
		public Formatter[] TotalPageFormatters { get; set; }

		public BaseSelector CurrenctPageSelector { get; set; }
		public Formatter[] CurrnetPageFormatters { get; set; }

		public bool IsTermination(Page page, BaseTargetUrlsBuilder creator)
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
			string totalStr = string.Empty;
			if (selector.Type == SelectorType.Enviroment)
			{
				if (SelectorUtils.Parse(TotalPageSelector) is EnviromentSelector enviromentSelector)
				{
					totalStr = SelectorUtils.GetEnviromentValue(enviromentSelector.Field, page, 0)?.ToString();
				}
			}
			else
			{
				totalStr = page.Selectable.Select(SelectorUtils.Parse(TotalPageSelector)).GetValue();
			}

			if (!string.IsNullOrEmpty(totalStr) && TotalPageFormatters != null)
			{
				foreach (var formatter in TotalPageFormatters)
				{
					totalStr = formatter.Formate(totalStr)?.ToString();
				}
			}

			if (string.IsNullOrEmpty(totalStr))
			{
				throw new SpiderException("The result of total selector is null.");
			}
			else
			{
				return totalStr;
			}
		}
	}

}
