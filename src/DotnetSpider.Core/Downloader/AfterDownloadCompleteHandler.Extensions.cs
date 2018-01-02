using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using HtmlAgilityPack;
using NLog;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 正常的解析TargetUrls是在Processor中实现的, 此处是用于一些特别情况如可能想添加多个解析器
	/// </summary>
	public class TargetUrlsHandler : AfterDownloadCompleteHandler
	{
		private readonly ITargetUrlsExtractor _targetUrlsExtractor;
		private readonly bool _extractByProcessor;

		public TargetUrlsHandler(ITargetUrlsExtractor targetUrlsExtractor, bool extractByProcessor = false)
		{
			_targetUrlsExtractor = targetUrlsExtractor ?? throw new ArgumentNullException(nameof(targetUrlsExtractor));
			_extractByProcessor = extractByProcessor;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (_targetUrlsExtractor == null)
			{
				return;
			}
			var requests = _targetUrlsExtractor.ExtractRequests(page, spider.Site);
			foreach (var request in requests)
			{
				page.AddTargetRequest(request);
			}
			if (!_extractByProcessor)
			{
				page.SkipExtractTargetUrls = !page.SkipExtractTargetUrls || page.SkipExtractTargetUrls;
			}
		}
	}

	/// <summary>
	/// 定时更新Cookie的处理器
	/// </summary>
	public class TimingUpdateCookieHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;

		private readonly int _dueTime;

		private DateTime _nextTime;

		public TimingUpdateCookieHandler(int dueTime, ICookieInjector injector)
		{
			if (dueTime <= 0)
			{
				throw new SpiderException("dueTime should be large than 0.");
			}
			_cookieInjector = injector ?? throw new SpiderException("CookieInjector should not be null.");
			_nextTime = DateTime.Now.AddSeconds(_dueTime);
			_dueTime = dueTime;
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

	/// <summary>
	/// 当下载的内容包含指定内容时, 直接跳过此链接
	/// </summary>
	public class SkipWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contains;

		public SkipWhenContainsHandler(params string[] contains)
		{
			_contains = contains;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var content = page.Content;
				page.Skip = _contains.Any(c => content.Contains(c));
			}
		}
	}

	/// <summary>
	/// 去除下载内容中的HTML标签
	/// </summary>
	public class PlainTextHandler : AfterDownloadCompleteHandler
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

	/// <summary>
	/// 所有内容转化成大写
	/// </summary>
	public class ToUpperHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.ToUpper();
			}
		}
	}

	/// <summary>
	/// 所有内容转化成小写
	/// </summary>
	public class ToLowerHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.ToLower();
			}
		}
	}

	/// <summary>
	/// 替换内容
	/// </summary>
	public class ReplaceHandler : AfterDownloadCompleteHandler
	{
		private readonly string _oldValue;
		private readonly string _newValue;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="oldValue">The string to be replaced.</param>
		/// <param name="newValue">The string to replace all occurrences of oldValue.</param>
		public ReplaceHandler(string oldValue, string newValue = "")
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

	/// <summary>
	/// Removes all leading and trailing white-space characters from the current content.
	/// </summary>
	public class TrimHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = page.Content.Trim();
			}
		}
	}

	/// <summary>
	/// Converts any escaped characters in current content.
	/// </summary>
	public class UnescapeHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				page.Content = Regex.Unescape(page.Content);
			}
		}
	}

	/// <summary>
	/// Searches the current content for all occurrences of a specified regular expression, using the specified matching options.
	/// </summary>
	public class RegexHandler : AfterDownloadCompleteHandler
	{
		private readonly string _pattern;
		private readonly RegexOptions _regexOptions;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="pattern">The regular expression pattern to match.</param>
		/// <param name="options">A bitwise combination of the enumeration values that specify options for matching.</param>
		public RegexHandler(string pattern, RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase)
		{
			_pattern = pattern;
			_regexOptions = options;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return;
			}

			string textValue = string.Empty;
			MatchCollection collection =
				Regex.Matches(page.Content, _pattern, _regexOptions);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			page.Content = textValue;
		}
	}

	/// <summary>
	/// 当包含指定内容时重试当前链接
	/// </summary>
	public class RetryWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contents;

		public RetryWhenContainsHandler(params string[] contents)
		{
			if (contents == null || contents.Length == 0)
			{
				throw new SpiderException("contents should not be empty/null.");
			}
			_contents = contents;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var content = page.Content;
				if (_contents.Any(c => content.Contains(c)))
				{
					page.AddTargetRequest(page.Request);
				}
			}
		}
	}

	/// <summary>
	/// 当包含指定内容时触发ADSL拨号
	/// </summary>
	public class RedialWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contents;

		public RedialWhenContainsHandler(params string[] contents)
		{
			_contents = contents;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var content = page.Content;
				var containContent = _contents.FirstOrDefault(c => content.Contains(c));
				if (containContent != null)
				{
					if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
					{
						Logger.AllLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
						spider.Exit();
					}
					page = Spider.AddToCycleRetry(page.Request, spider.Site);
					page.Exception = new DownloadException($"Downloaded content contains: {containContent}.");
				}
			}
		}
	}

	/// <summary>
	/// 当Page对象中的异常信息包含指定内容时触发ADSL拨号
	/// </summary>
	public class RedialWhenExceptionThrowHandler : AfterDownloadCompleteHandler
	{
		private readonly string _exceptionMessage;

		public RedialWhenExceptionThrowHandler(string exceptionMessage)
		{
			if (string.IsNullOrEmpty(exceptionMessage) || string.IsNullOrWhiteSpace(exceptionMessage))
			{
				throw new SpiderException("exceptionMessage should not be null or empty.");
			}
			_exceptionMessage = exceptionMessage;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && page.Exception != null)
			{
				if (string.IsNullOrEmpty(_exceptionMessage))
				{
					page.Exception = new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(_exceptionMessage))
				{
					if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
					{
						Logger.AllLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
						spider.Exit();
					}
					Spider.AddToCycleRetry(page.Request, spider.Site);
					page.Exception = new DownloadException("Download failed and redial finished already.");
				}
			}
		}
	}

	/// <summary>
	/// 当包含指定内容时触发ADSL拨号, 并且重新获取Cookie
	/// </summary>
	public class RedialAndUpdateCookieWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly ICookieInjector _cookieInjector;
		private readonly string[] _contents;

		public RedialAndUpdateCookieWhenContainsHandler(ICookieInjector cookieInjector, params string[] contents)
		{
			_cookieInjector = cookieInjector ?? throw new SpiderException("cookieInjector should not be null.");
			if (contents == null || contents.Length == 0)
			{
				throw new SpiderException("contents should not be null or empty.");
			}
			_contents = contents;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content))
			{
				var content = page.Content;
				var containContent = _contents.FirstOrDefault(c => content.Contains(c));
				if (containContent != null)
				{
					if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
					{
						spider.Exit();
					}
					Spider.AddToCycleRetry(page.Request, spider.Site);
					_cookieInjector.Inject(spider);
					page.Exception = new DownloadException($"Downloaded content contains: {containContent}.");
				}
			}
		}
	}

	/// <summary>
	/// 截取下载内容的处理器
	/// </summary>
	public class CutoutHandler : AfterDownloadCompleteHandler
	{
		private readonly string _startPart;
		private readonly string _endPart;
		private readonly int _startOffset;
		private readonly int _endOffset;

		public CutoutHandler(string startPart, string endPart, int startOffset = 0, int endOffset = 0)
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
