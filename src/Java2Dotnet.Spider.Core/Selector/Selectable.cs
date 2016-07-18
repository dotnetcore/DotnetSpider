using System.Collections.Generic;
using HtmlAgilityPack;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Utils;

namespace Java2Dotnet.Spider.Core.Selector
{
	public class Selectable : BaseSelectable
	{
		public Selectable(string text, string urlOrPadding, ContentType contentType)
		{
			switch (contentType)
			{
				case ContentType.Html:
					{
						HtmlDocument document = new HtmlDocument();
						document.OptionAutoCloseOnEnd = true;
						document.LoadHtml(text);

						if (!string.IsNullOrEmpty(urlOrPadding))
						{
							FixAllRelativeHrefs(document, urlOrPadding);
						}

						Elements = new List<dynamic> { document.DocumentNode.OuterHtml };
						break;
					}
				case ContentType.Json:
					{
						string json = string.IsNullOrEmpty(urlOrPadding) ? text : RemovePadding(text, urlOrPadding);
						Elements = new List<dynamic> { json };
						break;
					}
			}

		}

		public Selectable(List<dynamic> nodes)
		{
			Elements = nodes;
		}

		public override ISelectable Css(string selector)
		{
			return Select(Selectors.Css(selector));
		}

		public override ISelectable Css(string selector, string attrName)
		{
			var cssSelector = Selectors.Css(selector, attrName);
			return Select(cssSelector);
		}

		public override ISelectable SmartContent()
		{
			return Select(Selectors.SmartContent());
		}

		/// <summary>
		/// 仅用于Html查询
		/// </summary>
		/// <returns></returns>
		public override ISelectable Links()
		{
			return XPath(".//a/@href").Regex(@"((http|ftp|https)://)(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-,]*)?");
		}

		public override ISelectable XPath(string xpath)
		{
			return SelectList(Selectors.XPath(xpath));
		}

		public override ISelectable Select(ISelector selector)
		{
			if (selector != null)
			{
				List<dynamic> resluts = new List<dynamic>();
				foreach (var selectedNode in Elements)
				{
					var result = selector.Select(selectedNode);
					if (result != null)
					{
						resluts.Add(result);
					}
				}
				return new Selectable(resluts);
			}
			throw new SpiderExceptoin("Selector is null.");
		}

		public override ISelectable SelectList(ISelector selector)
		{
			if (selector != null)
			{
				List<dynamic> resluts = new List<dynamic>();
				foreach (var selectedNode in Elements)
				{
					var result = selector.SelectList(selectedNode);
					if (result != null)
					{
						resluts.AddRange(result);
					}
				}
				return new Selectable(resluts);
			}

			throw new SpiderExceptoin("Selector is null.");
		}

		public override IList<ISelectable> Nodes()
		{
			//return Elements.Select(element => new Selectable(element)).Cast<ISelectable>().ToList();
			List<ISelectable> reslut = new List<ISelectable>();
			foreach (var element in Elements)
			{
				reslut.Add(new Selectable(new List<dynamic>() { element }));
			}
			return reslut;
		}

		public override ISelectable JsonPath(string path)
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector(path);
			return SelectList(jsonPathSelector);
		}

		/// <summary>
		/// Remove padding for JSONP
		/// </summary>
		/// <param name="text"></param>
		/// <param name="padding"></param>
		/// <returns></returns>
		public string RemovePadding(string text, string padding)
		{
			if (string.IsNullOrEmpty(padding))
			{
				return text;
			}

			XTokenQueue tokenQueue = new XTokenQueue(text);
			tokenQueue.ConsumeWhitespace();
			tokenQueue.Consume(padding);
			tokenQueue.ConsumeWhitespace();
			return tokenQueue.ChompBalancedNotInQuotes('(', ')');
		}

		private void FixAllRelativeHrefs(HtmlDocument document, string url)
		{
			var nodes = document.DocumentNode.SelectNodes("//a[not(starts-with(@href,'http') or starts-with(@href,'https'))]");
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					if (node.Attributes["href"] != null)
					{
						node.Attributes["href"].Value = UrlUtils.CanonicalizeUrl(node.Attributes["href"].Value, url);
					}
				}
			}
		}
	}
}