using System;
using System.Text.RegularExpressions;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;

#else
using HtmlAgilityPack;
#endif

namespace DotnetSpider.Core.Downloader
{
    public abstract class DownloadCompleteHandler : Named, IDownloadCompleteHandler
    {
        public abstract bool Handle(Page page, ISpider spider);
    }

    public class UpdateCookieWhenContainsHandler : DownloadCompleteHandler
    {
        public ICookieInjector CookieInjector { get; set; }
        public string Content { get; set; }

        public override bool Handle(Page page, ISpider spider)
        {
            if (!string.IsNullOrEmpty(page.Content) && page.Content.Contains(Content))
            {
                CookieInjector?.Inject(spider);
            }
            throw new DownloadException($"Content downloaded contains string: {Content}.");
        }
    }

    public class TimerUpdateCookieHandler : DownloadCompleteHandler
    {
        public ICookieInjector CookieInjector { get; }
        public int Interval { get; }
        protected DateTime Next { get; set; }

        public TimerUpdateCookieHandler(int interval, ICookieInjector injector)
        {
            Interval = interval;
            CookieInjector = injector;
            Next = DateTime.Now.AddSeconds(Interval);
        }

        public override bool Handle(Page page, ISpider spider)
        {
            if (DateTime.Now > Next)
            {
                CookieInjector?.Inject(spider);
                Next = DateTime.Now.AddSeconds(Interval);
            }

            return true;
        }
    }

    public class SkipWhenContainsHandler : DownloadCompleteHandler
    {
        public string Content { get; set; }

        public override bool Handle(Page page, ISpider spider)
        {
            if (string.IsNullOrEmpty(page.Content))
            {
                return true;
            }
            if (page.Content.Contains(Content))
            {
                page.IsSkip = true;
                return false;
            }
            return true;
        }
    }

    public class MissTargetUrlWhenNotContainsHandler : DownloadCompleteHandler
    {
        public string Content { get; set; }

        public override bool Handle(Page page, ISpider spider)
        {
            if (string.IsNullOrEmpty(page.Content))
            {
                return true;
            }

            if (!page.Content.Contains(Content))
            {
                page.MissExtractTargetUrls = true;
                page.MissTargetUrls = true;
            }
            return true;
        }
    }

    public class SubContentHandler : DownloadCompleteHandler
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int StartOffset { get; set; } = 0;
        public int EndOffset { get; set; } = 0;

        public override bool Handle(Page p, ISpider spider)
        {
            string rawText = p.Content;

            int begin = rawText.IndexOf(Start, StringComparison.Ordinal);
            int end = rawText.IndexOf(End, begin, StringComparison.Ordinal);
            int length = end - begin;

            begin += StartOffset;
            length -= StartOffset;
            length -= EndOffset;
            length += End.Length;

            if (begin < 0 || length < 0)
            {
                throw new SpiderException("Sub content failed. Please check your settings.");
            }
            string newRawText = rawText.Substring(begin, length).Trim();
            p.Content = newRawText;

            return true;
        }
    }

    public class RemoveContentHandler : DownloadCompleteHandler
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int StartOffset { get; set; } = 0;
        public int EndOffset { get; set; } = 0;
        public bool RemoveAll { get; set; } = false;

        public override bool Handle(Page p, ISpider spider)
        {
            string rawText = p.Content;

            int begin = rawText.IndexOf(Start, StringComparison.Ordinal);
            if (begin > 0)
            {
                do
                {
                    int end = rawText.IndexOf(End, begin, StringComparison.Ordinal);
                    int length = end - begin;

                    begin += StartOffset;
                    length -= StartOffset;
                    length -= EndOffset;
                    length += End.Length;

                    if (begin < 0 || length < 0)
                    {
                        throw new SpiderException("Remove content failed. Please check your settings.");
                    }

                    rawText = rawText.Remove(begin, length);
                } while ((begin = rawText.IndexOf(Start, StringComparison.Ordinal)) > 0 && RemoveAll);
            }
            p.Content = rawText;
            return true;
        }
    }

    public class RemoveHtmlTagHandler : DownloadCompleteHandler
    {
        public override bool Handle(Page p, ISpider spider)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(p.Content);
            p.Content = htmlDocument.DocumentNode.InnerText;
            return true;
        }
    }

    public class ContentToUpperHandler : DownloadCompleteHandler
    {
        public override bool Handle(Page p, ISpider spider)
        {
            if (!string.IsNullOrEmpty(p.Content))
            {
                p.Content = p.Content.ToUpper();
            }
            return true;
        }
    }

    public class ContentToLowerHandler : DownloadCompleteHandler
    {
        public bool ToUpper { get; set; } = false;

        public override bool Handle(Page p, ISpider spider)
        {
            if (!string.IsNullOrEmpty(p.Content))
            {
                p.Content = p.Content.ToLower();
            }
            return true;
        }
    }

    public class CustomizeContentHandler : DownloadCompleteHandler
    {
        public bool Loop { get; set; } = true;
        public bool DisableNewLine { get; set; } = false;
        public string Start { get; set; }
        public string End { get; set; }
        public int StartOffset { get; set; } = 0;
        public int EndOffset { get; set; } = 0;
        public string TargetTag { get; set; } = "my_target";

        public override bool Handle(Page p, ISpider spider)
        {
            string rawText = p.Content;
            rawText = rawText.Replace("script", "div");
            if (DisableNewLine)
            {
                rawText = rawText.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            }
            int start = 0;
            if (Loop)
            {
                while (rawText.IndexOf(Start, start, StringComparison.Ordinal) != -1)
                {
                    rawText = AmendRawText(rawText, ref start);
                }
            }
            else
            {
                rawText = AmendRawText(rawText, ref start);
            }

            p.Content = rawText;
            return true;
        }

        private string AmendRawText(string rawText, ref int start)
        {
            int begin = rawText.IndexOf(Start, start, StringComparison.Ordinal) + StartOffset;
            if (begin >= 0)
            {
                int end = rawText.IndexOf(End, begin, StringComparison.Ordinal) + EndOffset;
                start = end;

                rawText = rawText.Insert(end, @"</div>");
                rawText = rawText.Insert(begin, "<div class=\"" + TargetTag + "\">");
            }
            return rawText;
        }
    }

    public class ReplaceContentHandler : DownloadCompleteHandler
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public override bool Handle(Page p, ISpider spider)
        {
            p.Content = p.Content?.Replace(OldValue, NewValue);
            return true;
        }
    }

    public class TrimContentHandler : DownloadCompleteHandler
    {
        public override bool Handle(Page p, ISpider spider)
        {
            if (!string.IsNullOrEmpty(p.Content))
            {
                p.Content = p.Content.Trim();
            }
            return true;
        }
    }

    public class UnescapeContentHandler : DownloadCompleteHandler
    {
        public override bool Handle(Page p, ISpider spider)
        {
            if (!string.IsNullOrEmpty(p.Content))
            {
                p.Content = Regex.Unescape(p.Content);
            }
            return true;
        }
    }

    public class RegexMatchContentHandler : DownloadCompleteHandler
    {
        public string Pattern { get; set; }

        public override bool Handle(Page p, ISpider spider)
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

    public class RetryWhenContainsHandler : DownloadCompleteHandler
    {
        public string Content { get; set; }

        public override bool Handle(Page p, ISpider spider)
        {
            if (string.IsNullOrEmpty(p.Content))
            {
                return true;
            }
            if (p.Content.Contains(Content))
            {
                Request r = (Request) p.Request.Clone();
                p.AddTargetRequest(r);
            }
            return true;
        }
    }
}