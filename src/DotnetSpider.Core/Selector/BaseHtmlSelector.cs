using System.Collections.Generic;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;
#else
using HtmlAgilityPack;
#endif

namespace DotnetSpider.Core.Selector
{
	public abstract class BaseHtmlSelector : ISelector
	{
		public virtual dynamic Select(dynamic text)
		{
			if (text != null)
			{
				if (text is string)
				{
					HtmlDocument document = new HtmlDocument {OptionAutoCloseOnEnd = true};
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
				var htmlNode = text as HtmlNode;
				if (htmlNode != null)
				{
					return SelectList(htmlNode);
				}
				else
				{
					HtmlDocument document = new HtmlDocument {OptionAutoCloseOnEnd = true};
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