using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;
#else
using HtmlAgilityPack;
#endif

namespace DotnetSpider.Core.Selector
{
	public class Selectable : BaseSelectable
	{
		public Selectable(string text, string urlOrPadding, ContentType contentType, params string[] domain)
		{
			switch (contentType)
			{
				case ContentType.Html:
					{
						HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
						document.LoadHtml(text);

						if (!string.IsNullOrEmpty(urlOrPadding))
						{
							FixAllRelativeHrefs(document, urlOrPadding);
						}

						if (domain != null && domain.Length > 0)
						{
							RemoveOutboundLinks(document, domain);
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
			var tmplinks = XPath("./descendant-or-self::a/@href").GetValues();
			var links = new List<dynamic>();
			foreach (var link in tmplinks)
			{
				Uri uri;
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out uri))
				{
					links.Add(link);
				}
			}
			return new Selectable(links);
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
			throw new SpiderException("Selector is null.");
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

			throw new SpiderException("Selector is null.");
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

			var images = document.DocumentNode.SelectNodes(".//img");
			if (images != null)
			{
				foreach (var image in images)
				{
					if (image.Attributes["src"] != null)
					{
						image.Attributes["src"].Value = UrlUtils.CanonicalizeUrl(image.Attributes["src"].Value, url);
					}
				}
			}
		}

		private void RemoveOutboundLinks(HtmlDocument document, params string[] domains)
		{
			var nodes = document.DocumentNode.SelectNodes(".//a");
			if (nodes != null)
			{
				List<HtmlNode> deleteNodes = new List<HtmlNode>();
				foreach (var node in nodes)
				{
					bool isMatch = false;
					foreach (var domain in domains)
					{
						if (System.Text.RegularExpressions.Regex.IsMatch(node.Attributes["href"].Value, domain))
						{
							isMatch = true;
							break;
						}
					}
					if (!isMatch)
					{
						deleteNodes.Add(node);
					}
				}
				foreach (var node in deleteNodes)
				{
					node.Remove();
				}
			}
		}
	}
}