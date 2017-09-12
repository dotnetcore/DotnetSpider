using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
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
