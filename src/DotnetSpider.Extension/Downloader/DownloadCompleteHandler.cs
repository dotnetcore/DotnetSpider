using System;
using System.Text.RegularExpressions;
//using System.Web;
using HtmlAgilityPack;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Redial;

namespace DotnetSpider.Extension.Downloader
{
	public abstract class DownloadCompleteHandler : Named, IDownloadCompleteHandler
	{
		public abstract void Handle(Page page);
	}

	#region Content Handler

	public class SubContentHandler : DownloadCompleteHandler
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override void Handle(Page p)
		{
			string rawText = p.Content;

			int begin = rawText.IndexOf(StartString, StringComparison.Ordinal);
			int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal);
			int length = end - begin;

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			length += EndString.Length;

			string newRawText = rawText.Substring(begin, length).Trim();
			p.Content = newRawText;
		}
	}

	public class RemoveContentHandler : DownloadCompleteHandler
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;
		public bool RemoveAll { get; set; } = false;

		public override void Handle(Page p)
		{
			string rawText = p.Content;

			int begin = rawText.IndexOf(StartString, StringComparison.Ordinal);
			if (begin > 0)
			{
				do
				{
					int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal);
					int length = end - begin;

					begin += StartOffset;
					length -= StartOffset;
					length -= EndOffset;
					length += EndString.Length;

					rawText = rawText.Remove(begin, length);
				} while ((begin = rawText.IndexOf(StartString, StringComparison.Ordinal)) > 0 && RemoveAll);
			}
			p.Content = rawText;
		}
	}

	public class RemoveContentHtmlTagHandler : DownloadCompleteHandler
	{
		public override void Handle(Page p)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(p.Content);
			p.Content = htmlDocument.DocumentNode.InnerText;
		}
	}

	public class ContentToUpperOrLowerHandler : DownloadCompleteHandler
	{
		public bool ToUpper { get; set; } = false;

		public override void Handle(Page p)
		{
			if (string.IsNullOrEmpty(p.Content))
			{
				return;
			}
			p.Content = ToUpper ? p.Content.ToUpper() : p.Content.ToLower();
		}
	}

	public class CustomTargetHandler : DownloadCompleteHandler
	{
		public bool Loop { get; set; } = true;
		public bool DisableNewLine { get; set; } = false;
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;
		public string TargetTag { get; set; } = "my_target";

		public override void Handle(Page p)
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
				while (rawText.IndexOf(StartString, start, StringComparison.Ordinal) != -1)
				{
					rawText = AmendRawText(rawText, ref start);
				}
			}
			else
			{
				rawText = AmendRawText(rawText, ref start);
			}

			p.Content = rawText;
		}

		private string AmendRawText(string rawText, ref int start)
		{
			int begin = rawText.IndexOf(StartString, start, StringComparison.Ordinal) + StartOffset;
			int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal) + EndOffset;
			start = end;

			rawText = rawText.Insert(end, @"</div>");
			rawText = rawText.Insert(begin, "<div class=\"" + TargetTag + "\">");

			return rawText;
		}
	}

	public class ReplaceContentHandler : DownloadCompleteHandler
	{
		public string OldValue { get; set; }
		public string NewValue { get; set; }

		public override void Handle(Page p)
		{
			p.Content = p.Content?.Replace(OldValue, NewValue);
		}
	}

	public class TrimContentHandler : DownloadCompleteHandler
	{
		public override void Handle(Page p)
		{
			p.Content = p.Content?.Trim();
		}
	}

	public class UnescapeContentHandler : DownloadCompleteHandler
	{
		public override void Handle(Page p)
		{
			p.Content = Regex.Unescape(p.Content);
		}
	}

	public class RegexMatchContentHandler : DownloadCompleteHandler
	{
		public string Pattern { get; set; }

		public override void Handle(Page p)
		{
			string textValue = string.Empty;
			MatchCollection collection = Regex.Matches(p.Content, Pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}
			p.Content = textValue;
		}
	}
	#endregion

	#region Redial Handler
	public class SkipWhenContainsIllegalStringHandler : DownloadCompleteHandler
	{
		public string ContainsString { get; set; }

		public override void Handle(Page page)
		{
			string rawText = page.Content;
			if (string.IsNullOrEmpty(rawText))
			{
				throw new DownloadException("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				page.IsSkip = true;
			}
		}
	}

	public class RedialWhenContainsIllegalStringHandler : DownloadCompleteHandler
	{
		public string ContainsString { get; set; }

		public override void Handle(Page page)
		{
			string rawText = page.Content;
			if (string.IsNullOrEmpty(rawText))
			{
				throw new DownloadException("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
				throw new DownloadException($"Content downloaded contains illegal string: {ContainsString}.");
			}
		}
	}

	public class RedialWhenExceptionThrowHandler : DownloadCompleteHandler
	{
		public string ExceptionMessage { get; set; } = string.Empty;

		public override void Handle(Page page)
		{
			if (page.Exception != null)
			{
				if (string.IsNullOrEmpty(ExceptionMessage))
				{
					throw new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(ExceptionMessage))
				{
					((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
					throw new DownloadException("Download failed and redial finished already.");
				}
			}
		}
	}

	public class CycleRedialHandler : DownloadCompleteHandler
	{
		public int RedialLimit { get; set; }
		public static int RequestedCount { get; set; }

		public override void Handle(Page page)
		{
			if (RedialLimit != 0)
			{
				lock (this)
				{
					++RequestedCount;

					if (RedialLimit > 0 && RequestedCount == RedialLimit)
					{
						RequestedCount = 0;

						((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
					}
				}
			}
		}
	}

	#endregion
}
