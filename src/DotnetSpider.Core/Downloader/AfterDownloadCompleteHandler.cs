using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using NLog;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Downloader
{
	public abstract class AfterDownloadCompleteHandler : Named, IAfterDownloadCompleteHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public abstract bool Handle(ref Page page, ISpider spider);
	}

	public class UpdateCookieWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public ICookieInjector CookieInjector { get; set; }

		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content) && page.Content.Contains(Content))
			{
				CookieInjector?.Inject(spider);
			}
			throw new SpiderException($"Content downloaded contains string: {Content}.");
		}
	}

	public class TimerUpdateCookieHandler : AfterDownloadCompleteHandler
	{
		public ICookieInjector CookieInjector { get; }

		public int Interval { get; }

		public DateTime Next { get; private set; }

		public TimerUpdateCookieHandler(int interval, ICookieInjector injector)
		{
			Interval = interval;
			CookieInjector = injector;
			Next = DateTime.Now.AddSeconds(Interval);
		}

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (DateTime.Now > Next)
			{
				CookieInjector?.Inject(spider);
				Next = DateTime.Now.AddSeconds(Interval);
			}

			return true;
		}
	}

	public class SkipWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page.Content))
			{
				return true;
			}
			if (page.Content.Contains(Content))
			{
				page.Skip = true;
				return false;
			}
			return true;
		}
	}

	public class SkipTargetUrlsWhenNotContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page.Content))
			{
				return true;
			}

			if (!page.Content.Contains(Content))
			{
				page.SkipExtractTargetUrls = true;
				page.SkipTargetUrls = true;
			}
			return true;
		}
	}

	public class RemoveHtmlTagHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page p, ISpider spider)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(p.Content);
			p.Content = htmlDocument.DocumentNode.InnerText;
			return true;
		}
	}

	public class ContentToUpperHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page p, ISpider spider)
		{
			if (!string.IsNullOrEmpty(p.Content))
			{
				p.Content = p.Content.ToUpper();
			}
			return true;
		}
	}

	public class ContentToLowerHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page p, ISpider spider)
		{
			if (!string.IsNullOrEmpty(p.Content))
			{
				p.Content = p.Content.ToLower();
			}
			return true;
		}
	}

	public class ReplaceContentHandler : AfterDownloadCompleteHandler
	{
		public string OldValue { get; set; }

		public string NewValue { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			page.Content = page.Content?.Replace(OldValue, NewValue);
			return true;
		}
	}

	public class TrimContentHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content))
			{
				page.Content = page.Content.Trim();
			}
			return true;
		}
	}

	public class UnescapeContentHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content))
			{
				page.Content = Regex.Unescape(page.Content);
			}
			return true;
		}
	}

	public class PatternMatchContentHandler : AfterDownloadCompleteHandler
	{
		public string Pattern { get; set; }

		public override bool Handle(ref Page p, ISpider spider)
		{
			string textValue = string.Empty;
			MatchCollection collection = Regex.Matches(p.Content, Pattern,
				RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			p.Content = textValue;
			return true;
		}
	}

	public class RetryWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page.Content))
			{
				return true;
			}
			if (page.Content.Contains(Content))
			{
				Request r = page.Request.Clone();
				page.AddTargetRequest(r);
			}
			return true;
		}
	}
}