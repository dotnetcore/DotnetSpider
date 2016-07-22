using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class PageHandler
	{
		[Flags]
		public enum Types
		{
			Sub,
			Remove,
			LoopRemove,
			RemoveHtml,
			CustomTarget,
			Replace,
			Case,
			Regex
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

		public override Types Type { get; internal set; } = Types.Remove;

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

	public class LoopRemovePageHandler : PageHandler
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override Types Type { get; internal set; } = Types.LoopRemove;

		public override void Customize(Page p)
		{
			try
			{
				string rawText = p.Content;

				int begin;
				while ((begin = rawText.IndexOf(StartString, StringComparison.Ordinal)) > 0)
				{
					int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal);
					int length = end - begin;

					begin += StartOffset;
					length -= StartOffset;
					length -= EndOffset;
					length += EndString.Length;

					rawText = rawText.Remove(begin, length);
				}

				p.Content = rawText;
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class RemoveHtmlTagPageHandler : PageHandler
	{
		public override Types Type { get; internal set; } = Types.RemoveHtml;

		public override void Customize(Page p)
		{
			try
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(p.Content);
				p.Content = htmlDocument.DocumentNode.InnerText;
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class CharacterCasePageHandler : PageHandler
	{
		public override Types Type { get; internal set; } = Types.Case;
		public bool ToUpper { get; set; } = false;

		public override void Customize(Page p)
		{
			try
			{
				if (ToUpper)
				{
					p.Content = p.Content.ToUpper();
				}
				else
				{
					p.Content = p.Content.ToLower();
				}
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class CustomTargetHandler : PageHandler
	{
		public bool Loop { get; set; } = true;
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
			catch
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}

		private string AmendRawText(string rawText,ref int start)
		{
			int begin = rawText.IndexOf(StartString, start, StringComparison.Ordinal) + StartOffset;
			int end = rawText.IndexOf(EndString, begin, StringComparison.Ordinal) + EndOffset;
			start = end;

			rawText = rawText.Insert(end, @"</div>");
			rawText = rawText.Insert(begin, "<div class=\"" + TargetTag + "\">");

			return rawText;
		}
	}

	public class ReplacePageHandler : PageHandler
	{
		public override Types Type { get; internal set; } = Types.Replace;
		public string OldValue { get; set; }
		public string NewValue { get; set; }

		public override void Customize(Page p)
		{
			try
			{
				p.Content = p.Content?.Replace(OldValue, NewValue);
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}

	public class RegexPageHandler : PageHandler
	{
		public override Types Type { get; internal set; } = Types.Regex;

		public string Pattern { get; set; }

		public override void Customize(Page p)
		{
			try
			{
				string textValue = string.Empty;
				MatchCollection collection = Regex.Matches(p.Content, Pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
				
				foreach (Match item in collection)
				{
					textValue += item.Value;
				}
				p.Content = textValue;
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Rawtext Invalid.");
			}
		}
	}
}
