using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using System.Linq;

namespace DotnetSpider.Extension.Downloader
{
	public interface ITargetUrlsBuilderTermination
	{
		bool IsTermination(Page page, BaseTargetUrlsBuilder creator);
	}

	public abstract class BaseTargetUrlsBuilder : IAfterDownloadCompleteHandler
	{
		public virtual bool Handle(ref Page page, ISpider spider)
		{
			if (page == null || string.IsNullOrEmpty(page.Content))
			{
				return true;
			}

			if (Termination == null || !Termination.IsTermination(page, this))
			{
				page.AddTargetRequests(GenerateRequests(page));
				page.SkipExtractedTargetUrls = true;
			}

			return true;
		}

		public ITargetUrlsBuilderTermination Termination { get; set; }

		protected abstract IList<Request> GenerateRequests(Page page);
	}

	public class IncrementTargetUrlsBuilder : BaseTargetUrlsBuilder
	{
		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public string PaggerString { get; protected set; }

		public int Interval { get; set; } = 1;

		protected Regex PaggerPattern { get; set; }

		public IncrementTargetUrlsBuilder(string paggerString, int interval = 1, ITargetUrlsBuilderTermination termination = null)
		{
			if (string.IsNullOrEmpty(paggerString))
			{
				throw new SpiderException("PaggerString should not be null.");
			}
			PaggerString = paggerString;
			Interval = interval;
			Termination = termination;
			PaggerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PaggerString, @"\d+")}");
		}

		protected virtual string GetCurrentPagger(string currentUrl)
		{
			return PaggerPattern.Match(currentUrl).Value;
		}

		protected string IncreasePageNum(string currentUrl)
		{
			var currentPaggerString = GetCurrentPagger(currentUrl);
			int currentPagger;
			var matches = RegexUtil.NumRegex.Matches(currentPaggerString);
			if (matches.Count == 0)
			{
				return null;
			}

			if (int.TryParse(matches[0].Value, out currentPagger))
			{
				var nextPagger = currentPagger + Interval;
				var next = RegexUtil.NumRegex.Replace(PaggerString, nextPagger.ToString());
				return currentUrl.Replace(currentPaggerString, next);
			}
			return null;
		}

		protected override IList<Request> GenerateRequests(Page page)
		{
			string newUrl = IncreasePageNum(page.Url);
			return string.IsNullOrEmpty(newUrl) ? null : new List<Request> { new Request(newUrl, page.Request.Extras) };
		}
	}

	public class RequestExtraTargetUrlsBuilder : BaseTargetUrlsBuilder
	{
		public string Field { get; set; }

		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public string PaggerString { get; protected set; }

		protected Regex PaggerPattern { get; set; }

		public RequestExtraTargetUrlsBuilder(string paggerString, string field, ITargetUrlsBuilderTermination termination = null)
		{
			if (string.IsNullOrEmpty(paggerString) || string.IsNullOrEmpty(field))
			{
				throw new SpiderException("PaggerString or field should not be null.");
			}
			PaggerString = paggerString;
			Field = field;
			Termination = termination;
			PaggerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PaggerString, @"\d+")}");
		}

		protected virtual string GetCurrentPagger(string currentUrl)
		{
			return PaggerPattern.Match(currentUrl).Value;
		}

		protected virtual string GenerateNewPaggerUrl(Page page)
		{
			var currentUrl = page.Url;
			var nextPagger = page.Request.GetExtra(Field)?.ToString();
			if (string.IsNullOrEmpty(nextPagger))
			{
				return null;
			}
			var currentPaggerString = GetCurrentPagger(currentUrl);
			int currentPagger;
			var matches = RegexUtil.NumRegex.Matches(currentPaggerString);
			if (matches.Count == 0)
			{
				return null;
			}

			if (int.TryParse(matches[0].Value, out currentPagger))
			{
				var next = RegexUtil.NumRegex.Replace(PaggerString, nextPagger.ToString());
				return currentUrl.Replace(currentPaggerString, next);
			}
			return null;
		}

		protected override IList<Request> GenerateRequests(Page page)
		{
			string newUrl = GenerateNewPaggerUrl(page);
			return string.IsNullOrEmpty(newUrl) ? null : new List<Request> { new Request(newUrl, page.Request.Extras) };
		}
	}

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
			if (page == null || string.IsNullOrEmpty(page.Content))
			{
				return false;
			}
			var totalStr = GetSelectorValue(page, TotalPageSelector, TotalPageFormatters);
			var currentStr = GetSelectorValue(page, CurrenctPageSelector, CurrnetPageFormatters);

			return currentStr == totalStr;
		}

		private string GetSelectorValue(Page page, BaseSelector selector, Formatter[] formatters)
		{
			string totalStr = string.Empty;
			if (selector.Type == SelectorType.Enviroment)
			{
				var enviromentSelector = SelectorUtils.Parse(TotalPageSelector) as EnviromentSelector;
				if (enviromentSelector != null)
				{
					totalStr = EntityExtractor.GetEnviromentValue(enviromentSelector.Field, page, 0);
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
					totalStr = formatter.Formate(totalStr);
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

	public class ContainsTermination : ITargetUrlsBuilderTermination
	{
		public string[] Contents { get; set; }

		public bool IsTermination(Page page, BaseTargetUrlsBuilder creator)
		{
			if (page == null || string.IsNullOrEmpty(page.Content))
			{
				return false;
			}

			return Contents.Any(c => page.Content.Contains(c));
		}
	}

	public class UnContainsTermination : ITargetUrlsBuilderTermination
	{
		public string[] Contents { get; set; }

		public bool IsTermination(Page page, BaseTargetUrlsBuilder creator)
		{
			if (page == null || string.IsNullOrEmpty(page.Content))
			{
				return false;
			}

			return !Contents.All(c => page.Content.Contains(c));
		}
	}
}
