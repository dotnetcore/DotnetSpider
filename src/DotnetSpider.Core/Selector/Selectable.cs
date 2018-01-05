using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;
using HtmlAgilityPack;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public class Selectable : BaseSelectable
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="text">被查询的文本</param>
		/// <param name="urlOrPadding">URL相对路径补充或者Json padding的去除</param>
		/// <param name="contentType">文本内容格式: Html, Json</param>
		/// <param name="domain">域名, 用于去除外链</param>
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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="nodes">被查询的元素</param>
		public Selectable(List<dynamic> nodes)
		{
			Elements = nodes;
		}

		/// <summary>
		/// 通过Css 选择器查找结果
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <returns>查询接口</returns>
		public override ISelectable Css(string css)
		{
			return Select(Selectors.Css(css));
		}

		/// <summary>
		/// 通过Css 选择器查找元素, 并取得属性的值
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <param name="attrName">查询到的元素的属性</param>
		/// <returns>查询接口</returns>
		public override ISelectable Css(string css, string attrName)
		{
			var cssSelector = Selectors.Css(css, attrName);
			return Select(cssSelector);
		}

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		public override ISelectable Links()
		{
			var tmplinks = XPath("./descendant-or-self::a/@href").GetValues();
			var links = new List<dynamic>();
			foreach (var link in tmplinks)
			{
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _))
				{
					links.Add(link);
				}
			}
			return new Selectable(links);
		}

		/// <summary>
		/// 通过XPath查找结果
		/// </summary>
		/// <param name="xpath">XPath 表达式</param>
		/// <returns>查询接口</returns>
		public override ISelectable XPath(string xpath)
		{
			return SelectList(Selectors.XPath(xpath));
		}

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
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

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
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

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		public override IList<ISelectable> Nodes()
		{
			List<ISelectable> reslut = new List<ISelectable>();
			foreach (var element in Elements)
			{
				reslut.Add(new Selectable(new List<dynamic>() { element }));
			}
			return reslut;
		}

		/// <summary>
		/// 通过JsonPath查找结果
		/// </summary>
		/// <param name="jsonPath">JsonPath 表达式</param>
		/// <returns>查询接口</returns>
		public override ISelectable JsonPath(string jsonPath)
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector(jsonPath);
			return SelectList(jsonPathSelector);
		}

		/// <summary>
		/// Remove padding for JSON
		/// </summary>
		/// <param name="text"></param>
		/// <param name="padding"></param>
		/// <returns></returns>
		private string RemovePadding(string text, string padding)
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
						node.Attributes["href"].Value = UrlUtil.CanonicalizeUrl(node.Attributes["href"].Value, url);
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
						image.Attributes["src"].Value = UrlUtil.CanonicalizeUrl(image.Attributes["src"].Value, url);
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
						var href = node.Attributes["href"]?.Value;
						if (!string.IsNullOrEmpty(href) && System.Text.RegularExpressions.Regex.IsMatch(href, domain))
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