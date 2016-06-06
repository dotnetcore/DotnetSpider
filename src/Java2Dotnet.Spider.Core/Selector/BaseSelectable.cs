using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Java2Dotnet.Spider.JLog;

namespace Java2Dotnet.Spider.Core.Selector
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
			RegexSelector regexSelector = Selectors.Regex(regex);
			return Select(regexSelector);
		}

		public ISelectable Regex(string regex, int group)
		{
			RegexSelector regexSelector = Selectors.Regex(regex, group);
			return Select(regexSelector);
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
					return Elements[0].ToString();
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
					result.Add(el.ToString());
				}
			}
			return result;
		}

		public abstract ISelectable Select(ISelector selector);

		public abstract ISelectable SelectList(ISelector selector);
	}
}
