using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Selector
{
	public class XPathSelector : BaseHtmlSelector
	{
		private readonly string _xpath;
		private static readonly Regex AttributeXPathRegex = new Regex(@"@[\w\s-]+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
		private readonly string _attribute;

		public XPathSelector(string xpathStr)
		{
			_xpath = xpathStr;
			//if (!string.IsNullOrEmpty(this.xpath))
			//{
			Match match = AttributeXPathRegex.Match(_xpath);
			if (!string.IsNullOrEmpty(match.Value) && _xpath.EndsWith(match.Value))
			{
				_attribute = match.Value.Replace("@", "");
				_xpath = _xpath.Replace("/" + match.Value, "");
			}
			//}
		}

		public override dynamic Select(HtmlAgilityPack.HtmlNode element)
		{
			var node = element.SelectSingleNode(_xpath);
			if (node != null)
			{
				if (HasAttribute())
				{
					return node.Attributes.Contains(_attribute) ? node.Attributes[_attribute].Value?.Trim() : null;
				}
				else
				{
					return node;
				}
			}
			return null;
		}

		public override List<dynamic> SelectList(HtmlAgilityPack.HtmlNode element)
		{
			List<dynamic> result = new List<dynamic>();
			var nodes = element.SelectNodes(_xpath);
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					if (!HasAttribute())
					{
						result.Add(node);
					}
					else
					{
						var attr = node.Attributes[_attribute];
						if (attr != null)
						{
							result.Add(attr.Value?.Trim());
						}
					}
				}
			}
			return result;
		}

		public override bool HasAttribute()
		{
			return !string.IsNullOrEmpty(_attribute);
		}
	}
}
