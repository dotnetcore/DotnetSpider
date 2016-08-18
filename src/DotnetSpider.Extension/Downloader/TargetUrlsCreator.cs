using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Common;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Downloader
{
	public interface ITargetUrlsCreatorStopper
	{
		bool NeedStop(Page page, BaseTargetUrlsCreator creator);
	}

	public abstract class BaseTargetUrlsCreator : IDownloadCompleteHandler
	{
		public void Handle(Page page)
		{
			if (Stopper != null)
			{
				if (!Stopper.NeedStop(page, this))
				{
					page.AddTargetRequests(GenerateRequests(page));
					page.MissExtractTargetUrls = true;
				}
			}
			else
			{
				page.AddTargetRequests(GenerateRequests(page));
				page.MissExtractTargetUrls = true;
			}
		}

		public abstract string GetCurrentPaggerString(string currentUrl);

		public ITargetUrlsCreatorStopper Stopper { get; set; }

		protected abstract IList<Request> GenerateRequests(Page page);
	}

	public abstract class BaseIncrementTargetUrlsCreator : BaseTargetUrlsCreator
	{
		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public string PaggerString { get; protected set; }

		public int Interval { get; set; }

		protected Regex PaggerPattern { get; set; }

		protected BaseIncrementTargetUrlsCreator(string paggerString, ITargetUrlsCreatorStopper stopper, int interval = 1)
		{
			if (string.IsNullOrEmpty(paggerString))
			{
				throw new SpiderException("PaggerString should not be null.");
			}
			PaggerString = paggerString;
			Interval = interval;
			Stopper = stopper;
			PaggerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PaggerString, @"\d+")}");
		}

		public override string GetCurrentPaggerString(string currentUrl)
		{
			return PaggerPattern.Match(currentUrl).Value;
		}

		public string IncreasePageNum(string currentUrl)
		{
			var current = GetCurrentPaggerString(currentUrl);
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);
			int nextIndex = currentIndex + Interval;
			var next = RegexUtil.NumRegex.Replace(PaggerString, nextIndex.ToString());
			return currentUrl.Replace(current, next);
		}
	}

	public class IncrementTargetUrlsCreator : BaseIncrementTargetUrlsCreator
	{
		protected override IList<Request> GenerateRequests(Page page)
		{
			string newUrl = IncreasePageNum(page.Url);
			return new List<Request> { new Request(newUrl, page.Request.Depth, page.Request.Extras) };
		}

		public IncrementTargetUrlsCreator(string paggerString, ITargetUrlsCreatorStopper stopper, int interval = 1) : base(paggerString, stopper, interval)
		{
		}

		public IncrementTargetUrlsCreator(string paggerString, int interval = 1) : base(paggerString, null, interval)
		{
		}
	}

	public class IncrementPostTargetUrlsCreator : IncrementTargetUrlsCreator
	{
		public IncrementPostTargetUrlsCreator(string paggerString, ITargetUrlsCreatorStopper stopper, int interval = 1) : base(paggerString, stopper, interval)
		{
		}

		public IncrementPostTargetUrlsCreator(string paggerString, int interval = 1) : base(paggerString, null, interval)
		{
		}

		protected override IList<Request> GenerateRequests(Page page)
		{
			return new List<Request>{
				new Request(page.Url, page.Request.Depth, page.Request.Extras)
				{
					Method = page.Request.Method,
					Origin = page.Request.Origin,
					PostBody = IncreasePageNum(page.Request.PostBody),
					Referer = page.Request.Referer
				}
			};
		}
	}

	public class PaggerStopper : ITargetUrlsCreatorStopper
	{
		public BaseSelector TotalPageSelector { get; set; }
		public List<Formatter> TotalPageFormatters { get; set; }

		public BaseSelector CurrenctPageSelector { get; set; }
		public List<Formatter> CurrnetPageFormatters { get; set; }

		public bool NeedStop(Page page, BaseTargetUrlsCreator creator)
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
	}

	public class LimitPageCountStopper : ITargetUrlsCreatorStopper
	{
		public int Limit { get; set; }

		public bool NeedStop(Page page, BaseTargetUrlsCreator creator)
		{
			string current = creator.GetCurrentPaggerString(page.Url);
			int currentIndex = int.Parse(RegexUtil.NumRegex.Match(current).Value);

			if (currentIndex == Limit)
			{
				return true;
			}
			return false;
		}
	}

	public class ContainContentsStopper : ITargetUrlsCreatorStopper
	{
		public List<string> Contents { get; set; }
		public bool IsContain { get; set; }

		public bool NeedStop(Page page, BaseTargetUrlsCreator creator)
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
			foreach (var stopper in Contents)
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

	public class CompareTimeInContentStopper : ITargetUrlsCreatorStopper
	{
		public bool IsBefore { get; set; } = true;
		public List<string> Times { get; set; } = new List<string>();
		public BaseSelector TimeSelector { get; set; }
		public List<Formatter> TimeFormatters { get; set; }

		public bool NeedStop(Page page, BaseTargetUrlsCreator creator)
		{
			var tmps = page.Selectable.SelectList(SelectorUtil.Parse(TimeSelector)).GetValues();
			if (tmps == null)
			{
				return true;
			}

			List<string> timeStrings = new List<string>();
			foreach (var c in tmps)
			{
				var s = c;
				if (TimeFormatters != null)
				{
					foreach (var formatter in TimeFormatters)
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
					foreach (var stopper in Times)
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
					foreach (var stopper in Times)
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
}
