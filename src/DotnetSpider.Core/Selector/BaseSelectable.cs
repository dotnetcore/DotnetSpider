using System.Collections.Generic;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;
#else
using HtmlAgilityPack;
#endif

namespace DotnetSpider.Core.Selector
{
	public abstract class BaseSelectable : ISelectable
	{
		public List<dynamic> Elements { get; set; }

		public abstract ISelectable XPath(string xpath);

		public abstract ISelectable Css(string selector);

		public abstract ISelectable Css(string selector, string attrName);

		public abstract ISelectable SmartContent();

		public abstract ISelectable Links();

		public abstract IList<ISelectable> Nodes();

		public abstract ISelectable JsonPath(string path);

		public ISelectable Regex(string regex)
		{
			return Select(Selectors.Regex(regex));
		}

		public ISelectable Regex(string regex, int group)
		{
			return Select(Selectors.Regex(regex, group));
		}

		public string GetValue(bool isPlainText)
		{
			if (Elements == null || Elements.Count == 0)
			{
				return null;
			}

			if (Elements.Count > 0)
			{
				if (Elements[0] is HtmlNode)
				{
					if (!isPlainText)
					{
						return Elements[0].InnerHtml;
					}
					else
					{
						return Elements[0].InnerText;
					}
				}
				else
				{
					if (!isPlainText)
					{
						return Elements[0].ToString();
					}
					else
					{
						var document = new HtmlDocument();
						document.LoadHtml(Elements[0].ToString());
						return document.DocumentNode.InnerText;
					}
				}
			}
			return null;
		}

		public List<string> GetValues(bool isPlainText)
		{
			List<string> result = new List<string>();
			foreach (var el in Elements)
			{
				if (el is HtmlNode)
				{
					if (!isPlainText)
					{
						result.Add(((HtmlNode)el).InnerHtml);
					}
					else
					{
						result.Add(((HtmlNode)el).InnerText.Trim());
					}
				}
				else
				{
					if (!isPlainText)
					{
						result.Add(el.ToString());
					}
					else
					{
						var document = new HtmlDocument();
						document.LoadHtml(el.ToString());
						result.Add(document.DocumentNode.InnerText);
					}
				}
			}
			return result;
		}

		public abstract ISelectable Select(ISelector selector);

		public abstract ISelectable SelectList(ISelector selector);
	}
}
