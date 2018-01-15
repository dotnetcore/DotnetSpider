using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Downloader
{
    /// <summary>
    /// We usually extract TargetUrls in <see cref="IPageProcessor"/>, here is for some special case like adding multiple <see cref="ITargetUrlsExtractor"/> etc.
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 正常的解析TargetUrls是在Processor中实现的, 此处是用于一些特别情况如可能想添加多个解析器
    /// </summary>
    public class TargetUrlsHandler : AfterDownloadCompleteHandler
    {
        private readonly ITargetUrlsExtractor _targetUrlsExtractor;
        private readonly bool _extractByProcessor;

        /// <summary>
        /// Construct a <see cref="TargetUrlsHandler"/> instance.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 构造方法
        /// </summary>
        /// <param name="targetUrlsExtractor">目标链接解析器 <see cref="ITargetUrlsExtractor"/></param>
        /// <param name="extractByProcessor">Processor是否还需要执行目标链接解析工作(Should <see cref="IPageProcessor"/> continue to execute <see cref="ITargetUrlsExtractor"/>)</param>
        public TargetUrlsHandler(ITargetUrlsExtractor targetUrlsExtractor, bool extractByProcessor = false)
        {
            _targetUrlsExtractor = targetUrlsExtractor ?? throw new ArgumentNullException(nameof(targetUrlsExtractor));
            _extractByProcessor = extractByProcessor;
        }

        /// <summary>
        /// Execute <see cref="ITargetUrlsExtractor"/>.
        /// </summary>
        /// <summary xml:lang="zh-CN">
		/// 执行目标链接解析器
        /// </summary>
        /// <param name="page">页面数据 <see cref="Page"/></param>
        /// <param name="downloader">下载器 <see cref="IDownloader"/></param>
        /// <param name="spider">爬虫 <see cref="ISpider"/></param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (_targetUrlsExtractor == null || page == null)
            {
                return;
            }

            var requests = _targetUrlsExtractor.ExtractRequests(page, spider.Site);
            foreach (var request in requests)
            {
                page.AddTargetRequest(request);
            }

            page.SkipExtractTargetUrls = !_extractByProcessor;
        }
    }

    /// <summary>
    /// Handler that regularly update cookies.
    /// </summary>
    /// <summary xml:lang="zh-CN">
	/// 定时更新Cookie的处理器
    /// </summary>
    public class TimingUpdateCookieHandler : AfterDownloadCompleteHandler
    {
        private readonly ICookieInjector _cookieInjector;
        private readonly int _interval;
        private DateTime _next;

        /// <summary>
        /// Construct a <see cref="TimingUpdateCookieHandler"/> instance.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 构造方法
        /// </summary>
        /// <param name="interval">间隔时间 interval time in second</param>
        /// <param name="injector">Cookie注入器 <see cref="ICookieInjector"/></param>
        /// <exception cref="SpiderException">dueTime should be large than 0.</exception>
        public TimingUpdateCookieHandler(int interval, ICookieInjector injector)
        {
            if (interval <= 0)
            {
                throw new SpiderException("dueTime should be large than 0.");
            }

            _cookieInjector = injector ?? throw new SpiderException("CookieInjector should not be null.");
            _next = DateTime.Now.AddSeconds(_interval);
            _interval = interval;
        }

        /// <summary>
        /// Update cookies regularly.
        /// </summary>
        /// <summary xml:lang="zh-CN">
		/// 定时更新Cookie
        /// </summary>
        /// <param name="page">页面数据 <see cref="Page"/></param>
        /// <param name="downloader">下载器 <see cref="IDownloader"/></param>
        /// <param name="spider">爬虫 <see cref="ISpider"/></param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (DateTime.Now > _next)
            {
                _cookieInjector.Inject(downloader, spider);
                _next = DateTime.Now.AddSeconds(_interval);
            }
        }
    }

    /// <summary>
    /// When <see cref="Page.Content"/> contains specified content, this <see cref="Page"/> will be skipped.
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 当下载的内容包含指定内容时, 直接跳过此链接
    /// </summary>
    public class SkipWhenContainsHandler : AfterDownloadCompleteHandler
    {
        private readonly string[] _contains;

        /// <param name="contains">包含的内容(contents to skip)</param>
        public SkipWhenContainsHandler(params string[] contains)
        {
            _contains = contains;
        }

        /// <summary>
        /// When <see cref="Page.Content"/> contains specified content, this <see cref="Page"/> will be skipped.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 如果页面数据包含指定内容, 跳过当前链接
        /// </summary>
        /// <param name="page">页面数据 <see cref="Page"/></param>
        /// <param name="downloader">下载器 <see cref="IDownloader"/></param>
        /// <param name="spider">爬虫 <see cref="ISpider"/></param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }

            var content = page.Content;
            page.Skip = _contains.Any(c => content.Contains(c));
        }
    }

    /// <summary>
    /// Handler that removes HTML tags in <see cref="Page.Content"/>.
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 去除下载内容中的HTML标签
    /// </summary>
    public class PlainTextHandler : AfterDownloadCompleteHandler
    {
        /// <summary>
        /// Remove HTML tags in <see cref="Page.Content"/>.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 去除下载内容中的HTML标签
        /// </summary>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(page.Content);
            page.Content = htmlDocument.DocumentNode.InnerText;
        }
    }

    /// <summary>
    /// Handler that make <see cref="Page.Content"/> to uppercase.
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 所有内容转化成大写
    /// </summary>
    public class ToUpperHandler : AfterDownloadCompleteHandler
    {
        /// <summary>
        /// make <see cref="Page.Content"/> to uppercase.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 所有内容转化成大写
        /// </summary>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            page.Content = page.Content.ToUpper();
        }
    }

    /// <summary>
    /// Handler that make <see cref="Page.Content"/> to lowercase.
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 所有内容转化成小写
    /// </summary>
    public class ToLowerHandler : AfterDownloadCompleteHandler
    {
        /// <summary>
        /// make <see cref="Page.Content"/> to lowercase.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 所有内容转化成小写
        /// </summary>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            page.Content = page.Content.ToLower();
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

        /// <summary>
        /// 替换内容
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            page.Content = page.Content.Replace(_oldValue, _newValue);
        }
    }

    /// <summary>
    /// Removes all leading and trailing white-space characters from the current content.
    /// </summary>
    public class TrimHandler : AfterDownloadCompleteHandler
    {
        /// <summary>
        /// Removes all leading and trailing white-space characters from the current content.
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            page.Content = page.Content.Trim();
        }
    }

    /// <summary>
    /// Converts any escaped characters in current content.
    /// </summary>
    public class UnescapeHandler : AfterDownloadCompleteHandler
    {
        /// <summary>
        /// Converts any escaped characters in current content.
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            page.Content = Regex.Unescape(page.Content);
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

        /// <summary>
        /// Searches the current content for all occurrences of a specified regular expression, using the specified matching options.
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }

            string textValue = string.Empty;
            MatchCollection collection = Regex.Matches(page.Content, _pattern, _regexOptions);

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

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="contents">包含的内容</param>
        public RetryWhenContainsHandler(params string[] contents)
        {
            if (contents == null || contents.Length == 0)
            {
                throw new SpiderException("contents should not be empty/null.");
            }

            _contents = contents;
        }

        /// <summary>
        /// 当包含指定内容时重试当前链接
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            var content = page.Content;
            if (_contents.Any(c => content.Contains(c)))
            {
                page.AddTargetRequest(page.Request);
            }
        }
    }

    /// <summary>
    /// 当包含指定内容时触发ADSL拨号
    /// </summary>
    public class RedialWhenContainsHandler : AfterDownloadCompleteHandler
    {
        private readonly string[] _contents;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="contents">包含的内容</param>
        public RedialWhenContainsHandler(params string[] contents)
        {
            _contents = contents;
        }

        /// <summary>
        /// 当包含指定内容时触发ADSL拨号
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            var content = page.Content;
            var containContent = _contents.FirstOrDefault(c => content.Contains(c));
            if (containContent != null)
            {
                if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
                {
                    Logger.Log(spider.Identity, "Exit program because redial failed.", Level.Error);
                    spider.Exit();
                }

                page = Spider.AddToCycleRetry(page.Request, spider.Site);
                page.Exception = new DownloadException($"Downloaded content contains: {containContent}.");
            }
        }
    }

    /// <summary>
    /// 当页面数据中的异常信息包含指定内容时触发ADSL拨号
    /// </summary>
    public class RedialWhenExceptionThrowHandler : AfterDownloadCompleteHandler
    {
        private readonly string _exceptionMessage;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public RedialWhenExceptionThrowHandler(string exceptionMessage)
        {
            if (string.IsNullOrWhiteSpace(exceptionMessage))
            {
                throw new SpiderException("exceptionMessage should not be null or empty.");
            }

            _exceptionMessage = exceptionMessage;
        }

        /// <summary>
        /// 当页面数据中的异常信息包含指定内容时触发ADSL拨号
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content) || page.Exception == null)
            {
                return;
            }
            if (page.Exception.Message.Contains(_exceptionMessage))
            {
                if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
                {
                    Logger.Log(spider.Identity, "Exit program because redial failed.", Level.Error);
                    spider.Exit();
                }

                Spider.AddToCycleRetry(page.Request, spider.Site);
                page.Exception = new DownloadException("Download failed and redial finished already.");
            }
        }
    }

    /// <summary>
    /// 当页面数据包含指定内容时触发ADSL拨号, 并且重新获取Cookie
    /// </summary>
    public class RedialAndUpdateCookiesWhenContainsHandler : AfterDownloadCompleteHandler
    {
        private readonly ICookieInjector _cookieInjector;
        private readonly string[] _contents;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="cookieInjector">Cookie注入器</param>
        /// <param name="contents">包含的内容</param>
        public RedialAndUpdateCookiesWhenContainsHandler(ICookieInjector cookieInjector, params string[] contents)
        {
            _cookieInjector = cookieInjector ?? throw new SpiderException("cookieInjector should not be null.");
            if (contents == null || contents.Length == 0)
            {
                throw new SpiderException("contents should not be null or empty.");
            }

            _contents = contents;
        }

        /// <summary>
        /// 当页面数据包含指定内容时触发ADSL拨号, 并且重新获取Cookie
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content))
            {
                return;
            }
            var content = page.Content;
            var containContent = _contents.FirstOrDefault(c => content.Contains(c));
            if (containContent != null)
            {
                if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
                {
                    spider.Exit();
                }

                Spider.AddToCycleRetry(page.Request, spider.Site);
                _cookieInjector.Inject(downloader, spider);
                page.Exception = new DownloadException($"Downloaded content contains: {containContent}.");
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

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="startPart">起始部分的内容</param>
        /// <param name="endPart">结束部分的内容</param>
        /// <param name="startOffset">开始截取的偏移</param>
        /// <param name="endOffset">结束截取的偏移</param>
        public CutoutHandler(string startPart, string endPart, int startOffset = 0, int endOffset = 0)
        {
            _startPart = startPart;
            _endOffset = endOffset;
            _endPart = endPart;
            _startOffset = startOffset;
        }

        /// <summary>
        /// 截取下载内容
        /// </summary>
        /// <param name="page">页面数据</param>
        /// <param name="downloader">下载器</param>
        /// <param name="spider">爬虫</param>
        public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.Content) || page.Skip)
            {
                return;
            }

            string rawText = page.Content;

            int begin = rawText.IndexOf(_startPart, StringComparison.Ordinal);

            if (begin < 0)
            {
                throw new SpiderException($"Cutout failed, can not find begin string: {_startPart}.");
            }

            int end = rawText.IndexOf(_endPart, begin, StringComparison.Ordinal);
            int length = end - begin;

            begin += _startOffset;
            length -= _startOffset;
            length -= _endOffset;
            length += _endPart.Length;

            if (begin < 0 || length < 0)
            {
                throw new SpiderException("Cutout failed. Please check your settings.");
            }

            string newRawText = rawText.Substring(begin, length).Trim();
            page.Content = newRawText;
        }
    }
}