using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Java2Dotnet.Spider.JLog;

namespace Java2Dotnet.Spider.Core.Selector
{
	public abstract class BaseSelectable : ISelectable
	{
#if !NET_CORE
		//protected static readonly ILog Logger = LogManager.GetLogger("BaseSelectable");
		protected static readonly ILog Logger = LogManager.GetLogger();
#else
		protected static readonly ILog Logger = LogManager.GetLogger();
#endif

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

		public dynamic Value
		{
			get
			{
				if (Elements == null || Elements.Count == 0)
				{
					return null;
				}

				if (Elements.Count == 1)
				{
					if (Elements[0] is HtmlNode)
					{
						return Elements[0].InnerHtml;
					}
					else
					{
						return Elements[0].ToString();
					}
				}

				return Elements.Select(selectedNode => selectedNode is HtmlNode ? Elements[0].InnerHtml : selectedNode.ToString()).ToList();
			}
		}

		public abstract ISelectable Select(ISelector selector);

		public abstract ISelectable SelectList(ISelector selector);
	}
}
