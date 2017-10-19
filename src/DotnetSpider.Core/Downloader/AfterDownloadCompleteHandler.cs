using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using NLog;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using System.Linq;

namespace DotnetSpider.Core.Downloader
{
	public abstract class AfterDownloadCompleteHandler : Named, IAfterDownloadCompleteHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public abstract void Handle(ref Page page, ISpider spider);
	}

	public sealed class UpdateCookieWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;
		private readonly string _content;

		public UpdateCookieWhenContainsContentHandler(ICookieInjector cookieInjector, string content)
		{
			_cookieInjector = cookieInjector;
			_content = content;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && page.Content.Contains(_content))
			{
				_cookieInjector?.Inject(spider);
			}
			throw new SpiderException($"Content downloaded contains string: {_content}.");
		}
	}

	public sealed class UpdateCookieTimerHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;

		private readonly int _dueTime;

		private DateTime _nextTime;

		public UpdateCookieTimerHandler(int dueTime, ICookieInjector injector)
		{
			_dueTime = dueTime;
			_cookieInjector = injector;
			_nextTime = DateTime.Now.AddSeconds(_dueTime);
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (DateTime.Now > _nextTime)
			{
				_cookieInjector?.Inject(spider);
				_nextTime = DateTime.Now.AddSeconds(_dueTime);
			}
		}
	}

	public sealed class SkipWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _content;

		public SkipWhenContainsContentHandler(string content)
		{
			_content = content;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			page.Skip = !string.IsNullOrEmpty(page?.Content) && page.Content.Contains(_content);
		}
	}

	public sealed class SkipTargetUrlsWhenNotContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _content;

		public SkipTargetUrlsWhenNotContainsContentHandler(string content)
		{
			_content = content;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !page.Content.Contains(_content))
			{
				page.SkipExtractTargetUrls = true;
				page.SkipTargetUrls = true;
			}
		}
	}

	public sealed class RemoveHtmlTagHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(page.Content);
				page.Content = htmlDocument.DocumentNode.InnerText;
			}
		}
	}

	public sealed class ContentToUpperHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.ToUpper();
			}
		}
	}

	public sealed class ContentToLowerHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.ToLower();
			}
		}
	}

	public sealed class ReplaceContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _oldValue;
		private readonly string _newValue;

		public ReplaceContentHandler(string oldValue, string newValue)
		{
			_oldValue = oldValue;
			_newValue = newValue;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.Replace(_oldValue, _newValue);
			}
		}
	}

	public sealed class TrimContentHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.Trim();
			}
		}
	}

	public sealed class UnescapeContentHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = Regex.Unescape(page.Content);
			}
		}
	}

	public sealed class PatternMatchContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _pattern;

		public PatternMatchContentHandler(string pattern)
		{
			_pattern = pattern;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return;
			}

			string textValue = string.Empty;
			MatchCollection collection =
				Regex.Matches(page.Content, _pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			page.Content = textValue;
		}
	}

	public sealed class RetryWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contents;

		public RetryWhenContainsContentHandler(params string[] contents)
		{
			if (contents == null || contents.Length == 0)
			{
				throw new SpiderException("Contents should not be empty/null.");
			}
			_contents = contents;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var tmpPage = page;
				if (_contents.Any(c => tmpPage.Content.Contains(c)))
				{
					Request r = page.Request.Clone();
					page.AddTargetRequest(r);
				}
			}
		}
	}

	public sealed class RedialWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _content;

		public RedialWhenContainsContentHandler(string content)
		{
			_content = content;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(_content) &&
				page.Content.Contains(_content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					Logger.MyLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
					spider.Exit();
				}
				page = Spider.AddToCycleRetry(page.Request, spider.Site);
				page.Exception = new DownloadException($"Content downloaded contains string: {_content}.");
			}
		}
	}

	public sealed class RedialWhenExceptionThrowHandler : AfterDownloadCompleteHandler
	{
		private readonly string _exceptionMessage;

		public RedialWhenExceptionThrowHandler(string exceptionMessage)
		{
			_exceptionMessage = exceptionMessage;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(_exceptionMessage) &&
				page.Exception != null)
			{
				if (string.IsNullOrEmpty(_exceptionMessage))
				{
					page.Exception = new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(_exceptionMessage))
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
		}
	}

	public sealed class RedialAndUpdateCookieWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;
		private readonly string _content;

		public RedialAndUpdateCookieWhenContainsContentHandler(ICookieInjector cookieInjector, string content)
		{
			_cookieInjector = cookieInjector;
			_content = content;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(_content) && _cookieInjector != null &&
				page.Content.Contains(_content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					spider.Exit();
				}
				Spider.AddToCycleRetry(page.Request, spider.Site);
				_cookieInjector?.Inject(spider);
				page.Exception = new DownloadException($"Content downloaded contains string: {_content}.");
			}
		}
	}

	public sealed class SubContentHandler : AfterDownloadCompleteHandler
	{
		private readonly string _startPart;
		private readonly string _endPart;
		private readonly int _startOffset;
		private readonly int _endOffset;

		public SubContentHandler(string startPart, string endPart, int startOffset = 0, int endOffset = 0)
		{
			_startPart = startPart;
			_endOffset = endOffset;
			_endPart = endPart;
			_startOffset = startOffset;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return;
			}

			string rawText = page.Content;

			int begin = rawText.IndexOf(_startPart, StringComparison.Ordinal);
			int end = rawText.IndexOf(_endPart, begin, StringComparison.Ordinal);
			int length = end - begin;

			begin += _startOffset;
			length -= _startOffset;
			length -= _endOffset;
			length += _endPart.Length;

			if (begin < 0 || length < 0)
			{
				throw new SpiderException("Sub content failed. Please check your settings.");
			}
			string newRawText = rawText.Substring(begin, length).Trim();
			page.Content = newRawText;
		}
	}
}