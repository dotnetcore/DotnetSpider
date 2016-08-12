using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Common;

namespace DotnetSpider.Extension.Model
{
	public abstract class TargetUrlsHandler
	{
		public abstract IList<Request> Handle(Page page);
	}

	public abstract class AbstractIncreasePageNumberTargetUrlsHandler : TargetUrlsHandler
	{
		/// <summary>
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
		public Selector TotalPageSelector { get; set; }
		public List<Formatter.Formatter> TotalPageFormatters { get; set; }

		public Selector CurrenctPageSelector { get; set; }
		public List<Formatter.Formatter> CurrnetPageFormatters { get; set; }

		public override bool CanStop(Page page)
		{
			int totalPage = -2000;
			if (TotalPageSelector != null)
			{
				string totalStr = page.Selectable.Select(SelectorUtil.Parse(TotalPageSelector)).GetValue();
				if (TotalPageFormatters != null)
				{
					foreach (var formatter in TotalPageFormatters)
					{
						totalStr = formatter.Formate(totalStr);
					}
				}
				if (!string.IsNullOrEmpty(totalStr))
				{
					totalPage = int.Parse(totalStr);
				}
			}
			int currentPage = -1000;
			if (CurrenctPageSelector != null)
			{
				string currentStr = page.Selectable.Select(SelectorUtil.Parse(CurrenctPageSelector)).GetValue();
				if (CurrnetPageFormatters != null)
				{
					foreach (var formatter in CurrnetPageFormatters)
					{
						currentStr = formatter.Formate(currentStr);
					}
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
		public bool IsBefore { get; set; } = true;
		public List<string> Stoppers { get; set; } = new List<string>();
		public Selector CurrenctPageSelector { get; set; }
		public List<Formatter.Formatter> CurrenctPageFormatters { get; set; }

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
			var current = page.Selectable.SelectList(SelectorUtil.Parse(CurrenctPageSelector)).GetValues();
			if (current == null)
			{
				return true;
			}

			List<string> timeStrings = new List<string>();
			foreach (var c in current)
			{
				var s = c;
				if (CurrenctPageFormatters != null)
				{
					foreach (var formatter in CurrenctPageFormatters)
					{
						s = formatter.Formate(s);
					}
				}
				timeStrings.Add(s);
			}

			foreach (var c in timeStrings)
			{
				var dt = DateTime.Parse(c);
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
