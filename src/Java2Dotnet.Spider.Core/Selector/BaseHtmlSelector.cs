using System.Collections.Generic;
using HtmlAgilityPack;

namespace Java2Dotnet.Spider.Core.Selector
{
	public abstract class BaseHtmlSelector : ISelector
	{
		public virtual dynamic Select(dynamic text)
		{
			if (text != null)
			{
				if (text is string)
				{
					HtmlDocument document = new HtmlDocument();
					document.OptionAutoCloseOnEnd = true;
					document.LoadHtml(text);
					return Select(document.DocumentNode);
				}
				else
				{
					return Select(text as HtmlNode);
				}
			}
			return null;
		}

		public virtual List<dynamic> SelectList(dynamic text)
		{
			if (text != null)
			{
				if (text is HtmlNode)
				{
					return SelectList(text as HtmlNode);

				}
				else
				{
					HtmlDocument document = new HtmlDocument();
					document.OptionAutoCloseOnEnd = true;
					document.LoadHtml(text);
					return SelectList(document.DocumentNode);
				}
			}
			else
			{
				return new List<dynamic>();
			}
		}

		public abstract bool HasAttribute();
		public abstract dynamic Select(HtmlNode element);
		public abstract List<dynamic> SelectList(HtmlNode element);
	}
}