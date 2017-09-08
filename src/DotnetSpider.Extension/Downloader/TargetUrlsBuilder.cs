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
using System.Net.Http;

namespace DotnetSpider.Extension.Downloader
{
	public interface ITargetUrlsBuilderTermination
	{
		/// <summary>
		/// Return true, skip all urls from target urls builder.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="creator"></param>
		/// <returns></returns>
		bool IsTermination(Page page, BaseTargetUrlsBuilder creator);
	}

	public abstract class BaseTargetUrlsBuilder : IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public string PaggerString { get; protected set; }

		public Regex PaggerPattern { get; protected set; }

		public virtual void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(PaggerString) && (Termination == null || !Termination.IsTermination(page, this)))
			{
				page.AddTargetRequests(GenerateRequests(page));
				page.SkipExtractTargetUrls = true;
			}
		}

		public ITargetUrlsBuilderTermination Termination { get; set; }

		protected abstract IList<Request> GenerateRequests(Page page);

		protected BaseTargetUrlsBuilder(string paggerString)
		{
			PaggerString = paggerString;
			PaggerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PaggerString, @"\d+")}");
		}

		public virtual string GetCurrentPagger(string currentUrlOrContent)
		{
			return PaggerPattern.Match(currentUrlOrContent).Value;
		}
	}

	public class IncrementTargetUrlsBuilder : BaseTargetUrlsBuilder
	{
		public int Interval { get; set; }

		public IncrementTargetUrlsBuilder(string paggerString, int interval = 1, ITargetUrlsBuilderTermination termination = null) : base(paggerString)
		{
			if (string.IsNullOrEmpty(paggerString))
			{
				throw new SpiderException("PaggerString should not be null.");
			}

			Interval = interval;
			Termination = termination;
		}

		protected string IncreasePageNum(string currentUrl)
		{
			var currentPaggerString = GetCurrentPagger(currentUrl);
			var matches = RegexUtil.NumRegex.Matches(currentPaggerString);
			if (matches.Count == 0)
			{
				return null;
			}

			if (int.TryParse(matches[0].Value, out var currentPagger))
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

		public RequestExtraTargetUrlsBuilder(string paggerString, string field, ITargetUrlsBuilderTermination termination = null) : base(paggerString)
		{
			if (string.IsNullOrEmpty(paggerString) || string.IsNullOrEmpty(field))
			{
				throw new SpiderException("PaggerString or field should not be null.");
			}
			Field = field;
			Termination = termination;
		}

		protected virtual string GenerateNewPaggerUrl(Page page)
		{
			var currentUrl = page.Url;
			var nextPagger = page.Request.GetExtra(Field)?.ToString();
			if (nextPagger != null)
			{
				var currentPaggerString = GetCurrentPagger(currentUrl);
				var matches = RegexUtil.NumRegex.Matches(currentPaggerString);
				if (matches.Count == 0)
				{
					return null;
				}

				if (int.TryParse(matches[0].Value, out _))
				{
					var next = RegexUtil.NumRegex.Replace(PaggerString, nextPagger.ToString());
					return currentUrl.Replace(currentPaggerString, next);
				}
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

		public bool IsTermination(Page page, BaseTargetUrlsBuilder builder)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return Contents.Any(c => page.Content.Contains(c));
		}
	}

	public class UnContainsTermination : ITargetUrlsBuilderTermination
	{
		public string[] Contents { get; set; }

		public bool IsTermination(Page page, BaseTargetUrlsBuilder builder)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return !Contents.All(c => page.Content.Contains(c));
		}
	}

	public class LimitPageNumTermination : ITargetUrlsBuilderTermination
	{
		public int Limit { get; set; }

		public LimitPageNumTermination(int limit)
		{
			Limit = limit;
		}

		public bool IsTermination(Page page, BaseTargetUrlsBuilder builder)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}
			var current = builder.GetCurrentPagger(page.Request.Method == HttpMethod.Get ? page.Url : page.Request.PostBody);
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);

			return currentIndex >= Limit;
		}
	}
}
