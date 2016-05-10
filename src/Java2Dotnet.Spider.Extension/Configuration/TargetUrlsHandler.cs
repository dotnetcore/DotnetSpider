using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class TargetUrlsHandler
	{
		[Flags]
		public enum Types
		{
			IncreasePageNumber,
			CustomLimitIncreasePageNumber,
            IncreasePageNumberWithStopper,
			IncreasePostPageNumberWithStopper
		}

		public abstract Types Type { get; internal set; }

		public abstract IList<Request> Handle(Page page);
	}

	public class IncreasePageNumberTargetUrlsHandler : TargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePageNumber;

		/// <summary>
		/// Like &s=44 或者 &page=1 或者 o1
		/// </summary>
		public string PageIndexString { get; set; }

		public int Interval { get; set; }

		public Selector TotalPageSelector { get; set; }

		public Selector CurrenctPageSelector { get; set; }

		public override IList<Request> Handle(Page page)
		{
			string pattern = $"{RegexUtil.NumRegex.Replace(PageIndexString, @"\d+")}";
			Regex regex = new Regex(pattern);
			string current = regex.Match(page.Url).Value;
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);
			int nextIndex = currentIndex + Interval;
			string next = RegexUtil.NumRegex.Replace(PageIndexString, nextIndex.ToString());

			int totalPage = -2000;
			if (TotalPageSelector != null)
			{
				string totalStr = page.Selectable.Select(SelectorUtil.GetSelector(TotalPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(totalStr))
				{
					totalPage = int.Parse(totalStr);
				}
			}
			int currentPage = -1000;
			if (CurrenctPageSelector != null)
			{
				string currentStr = page.Selectable.Select(SelectorUtil.GetSelector(CurrenctPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(currentStr))
				{
					currentPage = int.Parse(currentStr);
				}
			}
			if (currentPage == totalPage)
			{
				return new List<Request>();
			}

			return new List<Request> { new Request(page.Url.Replace(current, next), page.Request.Depth, page.Request.Extras) };
		}
	}

	public class CustomLimitIncreasePageNumberTargetUrlsHandler : IncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.CustomLimitIncreasePageNumber;

		public int To { get; set; }

		public override IList<Request> Handle(Page page)
		{
			string pattern = $"{RegexUtil.NumRegex.Replace(PageIndexString, @"\d+")}";
			Regex regex = new Regex(pattern);
			string current = regex.Match(page.Url).Value;
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);
			int nextIndex = currentIndex + Interval;
			string next = RegexUtil.NumRegex.Replace(PageIndexString, nextIndex.ToString());

			int totalPage = -2000;
			if (TotalPageSelector != null)
			{
				string totalStr = page.Selectable.Select(SelectorUtil.GetSelector(TotalPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(totalStr))
				{
					totalPage = int.Parse(totalStr);
				}
			}
			int currentPage = -1000;
			if (CurrenctPageSelector != null)
			{
				string currentStr = page.Selectable.Select(SelectorUtil.GetSelector(CurrenctPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(currentStr))
				{
					currentPage = int.Parse(currentStr);
				}
			}
			if (currentPage == totalPage || currentIndex == To)
			{
				return new List<Request>();
			}

			return new List<Request> { new Request(page.Url.Replace(current, next), page.Request.Depth, page.Request.Extras) };
		}
	}

	public class IncreasePageNumberWithStopperTargetUrlsHandler : IncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePageNumber;

		public string Stopper { get; set; } = "Spider Custom Stopper";

		public override IList<Request> Handle(Page page)
		{
			var urls = base.Handle(page);

			return page.Content.Contains(Stopper) ? new List<Request>() : urls;
		}
	}

	public class IncreasePostPageNumberWithStopperTargetUrlsHandler : IncreasePageNumberWithStopperTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePostPageNumberWithStopper;

		public override IList<Request> Handle(Page page)
		{
			if (page.Content.Contains(Stopper))
			{
				return new List<Request>();
			}

			string pattern = $"{RegexUtil.NumRegex.Replace(PageIndexString, @"\d+")}";
			Regex regex = new Regex(pattern);
			string current = regex.Match(page.Request.PostBody).Value;
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);
			int nextIndex = currentIndex + Interval;
			string next = RegexUtil.NumRegex.Replace(PageIndexString, nextIndex.ToString());

			int totalPage = -2000;
			if (TotalPageSelector != null)
			{
				string totalStr = page.Selectable.Select(SelectorUtil.GetSelector(TotalPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(totalStr))
				{
					totalPage = int.Parse(totalStr);
				}
			}
			int currentPage = -1000;
			if (CurrenctPageSelector != null)
			{
				string currentStr = page.Selectable.Select(SelectorUtil.GetSelector(CurrenctPageSelector)).GetValue();
				if (!string.IsNullOrEmpty(currentStr))
				{
					currentPage = int.Parse(currentStr);
				}
			}
			if (currentPage == totalPage)
			{
				return new List<Request>();
			}

			return new List<Request>
			{
				new Request(page.Url, page.Request.Depth, page.Request.Extras)
				{
					Method = page.Request.Method,
					Origin = page.Request.Origin,
					PostBody = page.Request.PostBody.Replace(current, next),
					Referer = page.Request.Referer
				}
			};
		}
	}
}
