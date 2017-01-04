using System;
using System.Text.RegularExpressions;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;
#else
using HtmlAgilityPack;
#endif

namespace DotnetSpider.Core.Downloader
{
	public interface IDownloadCompleteHandler
	{
		bool Handle(Page page);
	}

	public abstract class DownloadCompleteHandler : Named, IDownloadCompleteHandler
	{
		public abstract bool Handle(Page page);
	}

	#region Content Handler
	public class SkipWhenContainsIllegalStringHandler : DownloadCompleteHandler
	{
		public string ContainsString { get; set; }

		public override bool Handle(Page page)
		{
			string rawText = page.Content;
			if (string.IsNullOrEmpty(rawText))
			{
				throw new DownloadException("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				page.IsSkip = true;
				return false;
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

		public override bool Handle(Page p)
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

		public override bool Handle(Page p)
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

	public class RemoveContentHtmlTagHandler : DownloadCompleteHandler
	{
		public override bool Handle(Page p)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(p.Content);
			p.Content = htmlDocument.DocumentNode.InnerText;
			return true;
		}
	}

	public class ContentToUpperOrLowerHandler : DownloadCompleteHandler
	{
		public bool ToUpper { get; set; } = false;

		public override bool Handle(Page p)
		{
			if (string.IsNullOrEmpty(p.Content))
			{
				return false;
			}
			p.Content = ToUpper ? p.Content.ToUpper() : p.Content.ToLower();
			return true;
		}
	}

	public class CustomContentHandler : DownloadCompleteHandler
	{
		public bool Loop { get; set; } = true;
		public bool DisableNewLine { get; set; } = false;
		public string Start { get; set; }
		public string End { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;
		public string TargetTag { get; set; } = "my_target";

		public override bool Handle(Page p)
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

		public override bool Handle(Page p)
		{
			p.Content = p.Content?.Replace(OldValue, NewValue);
			return true;
		}
	}

	public class TrimContentHandler : DownloadCompleteHandler
	{
		public override bool Handle(Page p)
		{
			p.Content = p.Content?.Trim();
			return true;
		}
	}

	public class UnescapeContentHandler : DownloadCompleteHandler
	{
		public override bool Handle(Page p)
		{
			p.Content = Regex.Unescape(p.Content);
			return true;
		}
	}

	public class RegexMatchContentHandler : DownloadCompleteHandler
	{
		public string Pattern { get; set; }

		public override bool Handle(Page p)
		{
			string textValue = string.Empty;
			MatchCollection collection = Regex.Matches(p.Content, Pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			p.Content = textValue;
			return true;
		}
	}
	#endregion

	public class RetryWhenContainsIllegalStringHandler : DownloadCompleteHandler
	{
		public string ContainString { get; set; }

		public override bool Handle(Page p)
		{
			if (p.Content.Contains(ContainString))
			{
				Request r = (Request)p.Request.Clone();
				p.AddTargetRequest(r);
			}
			return true;
		}
	}
}
