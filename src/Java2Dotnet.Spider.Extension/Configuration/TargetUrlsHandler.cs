using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class TargetUrlsHandler
	{
		[Flags]
		public enum Types
		{
			IncreasePageNumber,
			IncreasePostPageNumber,
			CustomLimitIncreasePageNumber,
			IncreasePageNumberWithStopper,
			IncreasePostPageNumberWithStopper,
			IncreasePostPageNumberTimeStopper,
			IncreasePageNumberTimeStopper,
		}

		public abstract Types Type { get; internal set; }

		public abstract IList<Request> Handle(Page page);
	}

	public abstract class AbstractIncreasePageNumberTargetUrlsHandler : TargetUrlsHandler
	{
		/// <summary>
		/// Like &s=44 或者 &page=1 或者 o1
		/// </summary>
		public string PageIndexString { get; set; }

		public int Interval { get; set; }

		public abstract bool CanStop(Page page);

		public string IncreasePageNum(string targetString, out string current, out string next)
		{
			string pattern = $"{RegexUtil.NumRegex.Replace(PageIndexString, @"\d+")}";
			Regex regex = new Regex(pattern);
			current = regex.Match(targetString).Value;
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);
			int nextIndex = currentIndex + Interval;
			next = RegexUtil.NumRegex.Replace(PageIndexString, nextIndex.ToString());
			return targetString.Replace(current, next);
		}
	}

	public class IncreasePageNumberTargetUrlsHandler : AbstractIncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePageNumber;

		public Selector TotalPageSelector { get; set; }
		public Formatter TotalPageFormatter { get; set; }

		public Selector CurrenctPageSelector { get; set; }
		public Formatter CurrnetPageFormatter { get; set; }

		public override bool CanStop(Page page)
		{
			int totalPage = -2000;
			if (TotalPageSelector != null)
			{
				string totalStr = page.Selectable.Select(SelectorUtil.GetSelector(TotalPageSelector)).GetValue();
				if (TotalPageFormatter != null)
				{
					totalStr = TotalPageFormatter.Formate(totalStr);
				}
				if (!string.IsNullOrEmpty(totalStr))
				{
					totalPage = int.Parse(totalStr);
				}
			}
			int currentPage = -1000;
			if (CurrenctPageSelector != null)
			{
				string currentStr = page.Selectable.Select(SelectorUtil.GetSelector(CurrenctPageSelector)).GetValue();
				if (CurrnetPageFormatter != null)
				{
					currentStr = CurrnetPageFormatter.Formate(currentStr);
				}
				if (!string.IsNullOrEmpty(currentStr))
				{
					currentPage = int.Parse(currentStr);
				}
			}
			if (currentPage == totalPage)
			{
				return true;
			}
			return false;
		}

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			string newUrl = IncreasePageNum(page.Url, out current, out next);

			var canStop = CanStop(page);

			return canStop ? new List<Request>() : new List<Request> { new Request(newUrl, page.Request.Depth, page.Request.Extras) };
		}
	}

	public class IncreasePostPageNumberTargetUrlsHandler : IncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePostPageNumber;

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			IncreasePageNum(page.Request.PostBody, out current, out next);

			var canStop = CanStop(page);

			var newUrlList = new List<Request>{
				new Request(page.Url, page.Request.Depth, page.Request.Extras)
				{
					Method = page.Request.Method,
					Origin = page.Request.Origin,
					PostBody = page.Request.PostBody.Replace(current, next),
					Referer = page.Request.Referer
				}
			};
			return canStop ? new List<Request>() : newUrlList;
		}
	}

	public class CustomLimitIncreasePageNumberTargetUrlsHandler : AbstractIncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.CustomLimitIncreasePageNumber;

		public int To { get; set; }

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			string newUrl = IncreasePageNum(page.Url, out current, out next);

			var canStop = CanStop(page);

			return canStop ? new List<Request>() : new List<Request> { new Request(newUrl, page.Request.Depth, page.Request.Extras) };
		}

		public override bool CanStop(Page page)
		{
			string pattern = $"{RegexUtil.NumRegex.Replace(PageIndexString, @"\d+")}";
			Regex regex = new Regex(pattern);
			string current = regex.Match(page.Url).Value;
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);

			if (currentIndex == To)
			{
				return true;
			}
			return false;
		}
	}

	public class IncreasePageNumberWithStopperTargetUrlsHandler : AbstractIncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePageNumberWithStopper;

		public bool IsContain { get; set; } = true;
		public List<string> Stoppers { get; set; } = new List<string>();

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			string newUrl = IncreasePageNum(page.Url, out current, out next);

			var canStop = CanStop(page);

			return canStop ? new List<Request>() : new List<Request> { new Request(newUrl, page.Request.Depth, page.Request.Extras) };
		}

		public override bool CanStop(Page page)
		{
			bool canStop;
			if (IsContain)
			{
				canStop = false;
			}
			else
			{
				canStop = true;
			}
			foreach (var stopper in Stoppers)
			{
				if (IsContain)
				{
					if (page.Content.Contains(stopper))
					{
						canStop = true;
					}
				}
				else
				{
					if (page.Content.Contains(stopper))
					{
						canStop = false;
					}
				}
			}
			return canStop;
		}
	}

	public class IncreasePostPageNumberWithStopperTargetUrlsHandler : IncreasePageNumberWithStopperTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePostPageNumberWithStopper;

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			IncreasePageNum(page.Request.PostBody, out current, out next);

			var canStop = CanStop(page);

			var newUrlList = new List<Request>{
				new Request(page.Url, page.Request.Depth, page.Request.Extras)
				{
					Method = page.Request.Method,
					Origin = page.Request.Origin,
					PostBody = page.Request.PostBody.Replace(current, next),
					Referer = page.Request.Referer
				}
			};
			return canStop ? new List<Request>() : newUrlList;
		}
	}

	public class IncreasePageNumbeTimeStopperTargetUrlsHandler : AbstractIncreasePageNumberTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePageNumberTimeStopper;

		public bool IsBefore { get; set; } = true;
		public List<string> Stoppers { get; set; } = new List<string>();
		public Selector CurrenctPageSelector { get; set; }

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			string newUrl = IncreasePageNum(page.Url, out current, out next);

			var canStop = CanStop(page);

			return canStop ? new List<Request>() : new List<Request> { new Request(newUrl, page.Request.Depth, page.Request.Extras) };
		}

		public override bool CanStop(Page page)
		{
			var current = page.Selectable.SelectList(SelectorUtil.GetSelector(CurrenctPageSelector)).GetValues();
			if (current == null)
			{
				return true;
			}
			foreach (var c in (List<string>)current)
			{
				var dt = DateTime.Parse(c.ToString());
				if (IsBefore)
				{
					foreach (var stopper in Stoppers)
					{
						var stopDate = DateTime.Parse(stopper);
						if (dt < stopDate)
						{
							return true;
						}
					}
				}
				else
				{
					foreach (var stopper in Stoppers)
					{
						var stopDate = DateTime.Parse(stopper);
						if (dt > stopDate)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public class IncreasePostPageNumberTimeStopperTargetUrlsHandler : IncreasePageNumbeTimeStopperTargetUrlsHandler
	{
		public override Types Type { get; internal set; } = Types.IncreasePostPageNumberTimeStopper;

		public override IList<Request> Handle(Page page)
		{
			string current;
			string next;
			IncreasePageNum(page.Request.PostBody, out current, out next);

			var canStop = CanStop(page);

			var newUrlList = new List<Request>{
				new Request(page.Url, page.Request.Depth, page.Request.Extras)
				{
					Method = page.Request.Method,
					Origin = page.Request.Origin,
					PostBody = page.Request.PostBody.Replace(current, next),
					Referer = page.Request.Referer
				}
			};
			return canStop ? new List<Request>() : newUrlList;
		}
	}
}
