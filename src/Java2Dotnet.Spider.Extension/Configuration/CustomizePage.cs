using System;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class PageHandler
	{
		[Flags]
		public enum Types
		{
			Sub
		}

		public abstract Types Type { get; internal set; }

		public abstract void Customize(Page page);
	}

	public class SubCustomizePage : PageHandler
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
				int end = rawText.IndexOf(EndString, StringComparison.Ordinal);
				int length = end - begin;

				begin += StartOffset;
				length -= EndOffset;

				string newRawText = rawText.Substring(begin, length).Trim();
				p.Content = newRawText;
			}
			catch
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}
}
