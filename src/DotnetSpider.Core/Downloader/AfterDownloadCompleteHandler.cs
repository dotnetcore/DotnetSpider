using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using NLog;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Core.Redial;
using System.Linq;

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
			if (page != null && !string.IsNullOrEmpty(page.Content) && page.Content.Contains(Content))
			{
				CookieInjector?.Inject(spider);
			}
			throw new SpiderException($"Content downloaded contains string: {Content}.");
		}
	}

	public class UpdateCookieTimerHandler : AfterDownloadCompleteHandler
	{
		public ICookieInjector CookieInjector { get; }

		public int DueTime { get; }

		public DateTime NextTime { get; private set; }

		public UpdateCookieTimerHandler(int dueTime, ICookieInjector injector)
		{
			DueTime = dueTime;
			CookieInjector = injector;
			NextTime = DateTime.Now.AddSeconds(DueTime);
		}

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (DateTime.Now > NextTime)
			{
				CookieInjector?.Inject(spider);
				NextTime = DateTime.Now.AddSeconds(DueTime);
			}

			return true;
		}
	}

	public class SkipWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && string.IsNullOrEmpty(page.Content))
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
			if (page != null && string.IsNullOrEmpty(page.Content))
			{
				return true;
			}

			if (!page.Content.Contains(Content))
			{
				page.SkipExtractedTargetUrls = true;
				page.SkipTargetUrls = true;
			}
			return true;
		}
	}

	public class RemoveHtmlTagHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(page.Content);
				page.Content = htmlDocument.DocumentNode.InnerText;
			}
			return true;
		}
	}

	public class ContentToUpperHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				page.Content = page.Content.ToUpper();
			}
			return true;
		}
	}

	public class ContentToLowerHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				page.Content = page.Content.ToLower();
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
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				page.Content = page.Content.Replace(OldValue, NewValue);
			}

			return true;
		}
	}

	public class TrimContentHandler : AfterDownloadCompleteHandler
	{
		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && !string.IsNullOrEmpty(page.Content))
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
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				page.Content = Regex.Unescape(page.Content);
			}
			return true;
		}
	}

	public class PatternMatchContentHandler : AfterDownloadCompleteHandler
	{
		public string Pattern { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page != null && !string.IsNullOrEmpty(page.Content))
			{
				return true;
			}

			string textValue = string.Empty;
			MatchCollection collection = Regex.Matches(page.Content, Pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			page.Content = textValue;
			return true;
		}
	}

	public class RetryWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string[] Contents { get; set; }

		public RetryWhenContainsContentHandler(params string[] contents)
		{
			if (contents == null || contents.Length == 0)
			{
				throw new SpiderException("Contents should not be empty/null.");
			}
			Contents = contents;
		}

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page == null || string.IsNullOrEmpty(page.Content))
			{
				return true;
			}
			var tmpPage = page;
			if (Contents.Any(c => tmpPage.Content.Contains(c)))
			{
				Request r = page.Request.Clone();
				page.AddTargetRequest(r);
			}
			return true;
		}
	}

	public class RedialWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content) && page.Content.Contains(Content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					Logger.MyLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
					spider.Exit();
				}
				page = Spider.AddToCycleRetry(page.Request, spider.Site);
				page.Exception = new DownloadException($"Content downloaded contains string: {Content}.");
			}
			return true;
		}
	}

	public class RedialWhenExceptionThrowHandler : AfterDownloadCompleteHandler
	{
		public string ExceptionMessage { get; set; } = string.Empty;

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page.Exception != null)
			{
				if (string.IsNullOrEmpty(ExceptionMessage))
				{
					page.Exception = new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(ExceptionMessage))
				{
					if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
					{
						Logger.MyLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
						spider.Exit();
					}
					Spider.AddToCycleRetry(page.Request, spider.Site);
					page.Exception = new DownloadException("Download failed and redial finished already.");
				}
			}
			return true;
		}
	}

	public class RedialAndUpdateCookieWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public ICookieInjector CookieInjector { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content) && CookieInjector != null && page.Content.Contains(Content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					spider.Exit();
				}
				Spider.AddToCycleRetry(page.Request, spider.Site);
				CookieInjector?.Inject(spider);
				page.Exception = new DownloadException($"Content downloaded contains string: {Content}.");
			}
			return true;
		}
	}

	public class CycleRedialHandler : AfterDownloadCompleteHandler
	{
		public int RedialLimit { get; set; }

		public static int RequestedCount { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (RedialLimit != 0)
			{
				lock (this)
				{
					++RequestedCount;

					if (RedialLimit > 0 && RequestedCount == RedialLimit)
					{
						RequestedCount = 0;
						Spider.AddToCycleRetry(page.Request, spider.Site);
						if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
						{
							spider.Exit();
						}
					}
				}
			}
			return true;
		}
	}
}