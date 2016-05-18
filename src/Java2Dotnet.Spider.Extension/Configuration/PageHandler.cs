using System;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class PageHandler
	{
		[Flags]
		public enum Types
		{
			Sub,
			CustomTarget
		}

		public abstract Types Type { get; internal set; }

		public abstract void Customize(Page page);
	}

	public class SubPageHandler : PageHandler
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override Types Type { get; internal set; } = Types.Sub;

		public override void Customize(Page p)
		{
			try
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
			catch
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class RemovePageHandler : PageHandler
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override Types Type { get; internal set; } = Types.Sub;

		public override void Customize(Page p)
		{
			try
			{
				string rawText = p.Content;

				int begin = rawText.IndexOf(StartString, StringComparison.Ordinal);
				int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal);
				int length = end - begin;

				begin += StartOffset;
				length -= StartOffset;
				length -= EndOffset;
				length += EndString.Length;

				string newRawText = rawText.Remove(begin, length);
				p.Content = newRawText;
			}
			catch
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class CustomTargetHandler : PageHandler
	{
		public bool DisableNewLine { get; set; } = false;
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;
		public string TargetTag { get; set; } = "my_target";

		public override Types Type { get; internal set; } = Types.CustomTarget;

		public override void Customize(Page p)
		{
			try
			{
				string rawText = p.Content;
				rawText = rawText.Replace("script", "div");
				if (DisableNewLine)
				{
					rawText = rawText.Replace("\r", "").Replace("\n", "").Replace("\t", "");
				}
				int start = 0;
				while (rawText.IndexOf(StartString, start, StringComparison.Ordinal) != -1)
				{
					int begin = rawText.IndexOf(StartString, start, StringComparison.Ordinal) + StartOffset;
					int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal) + EndOffset;
					start = end;

					rawText = rawText.Insert(begin, "<div class=\"" + TargetTag + "\">");
					rawText = rawText.Insert(end, @"</div>");
				}

				p.Content = rawText;
			}
			catch
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}
}
