using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.Extraction
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public class Selectable : AbstractSelectable
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="html">Html</param>
		public Selectable(string html) : this(html, null, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="html">Html</param>
		/// <param name="url">URL相对路径补充</param>
		/// <param name="domains">域名, 用于去除外链</param>
		public Selectable(string html, string url, params string[] domains)
		{
			HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
			document.LoadHtml(html);

			if (!string.IsNullOrWhiteSpace(url))
			{
				FixAllRelativeHrefs(document, url);
			}

			if (domains != null && domains.Length > 0)
			{
				RemoveOutboundLinks(document, domains);
			}

			Elements = new List<dynamic> { document.DocumentNode.OuterHtml };
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="json">Json</param>
		/// <param name="padding">需要去除的 Json padding</param>
		public Selectable(string json, string padding)
		{
			json = string.IsNullOrWhiteSpace(json) ? json : RemovePadding(json, padding);
			Elements = new List<dynamic> { json };
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
		/// 通过共用属性查找进村
		/// </summary>
		/// <param name="field">属性名称</param>
		/// <returns>查询结果</returns>
		public override dynamic Enviroment(string field)
		{
			var key = field.ToLower();
			switch (key)
			{
				case "now":
					{
						return DateTime.Now;
					}
				case "monday":
					{
						var now = DateTime.Now;
						int i = now.DayOfWeek - DayOfWeek.Monday == -1 ? 6 : -1;
						TimeSpan ts = new TimeSpan(i, 0, 0, 0);
						return now.Subtract(ts).Date;
					}
				case "today":
					{
						return DateTime.Now.Date;
					}
				case "monthly":
					{
						var now = DateTime.Now;
						return now.AddDays(now.Day * -1 + 1);
					}
				default:
					{
						return Properties.ContainsKey(field) ? Properties[field] : null;
					}
			}
		}

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		public override ISelectable Links()
		{
			var links = XPath("./descendant-or-self::*/@href").GetValues();
			var sourceLinks = XPath("./descendant-or-self::*/@src").GetValues();
			var results = new HashSet<dynamic>();
			foreach (var link in links)
			{
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _))
				{
					results.Add(link);
				}
			}
			foreach (var link in sourceLinks)
			{
				if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _))
				{
					results.Add(link);
				}
			}
			return new Selectable(results.ToList());
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
			throw new ExtractionException($"{nameof(selector)} is null.");
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

			throw new ExtractionException($"{nameof(selector)} is null.");
		}

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		public override IEnumerable<ISelectable> Nodes()
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
		/// 计算最终的URL
		/// </summary>
		/// <param name="url">Base uri</param>
		/// <param name="refer">Relative uri</param>
		/// <returns>最终的URL</returns>
		public static string CanonicalizeUrl(string url, string refer)
		{
			try
			{
				Uri bas = new Uri(refer);
				Uri abs = new Uri(bas, url);
				return abs.AbsoluteUri;
			}
			catch (Exception)
			{
				return url;
			}
		}

		/// <summary>
		/// Remove padding for JSON
		/// </summary>
		/// <param name="text"></param>
		/// <param name="padding"></param>
		/// <returns></returns>
		private string RemovePadding(string text, string padding)
		{
			if (string.IsNullOrWhiteSpace(padding))
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
						node.Attributes["href"].Value = CanonicalizeUrl(node.Attributes["href"].Value, url);
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
						image.Attributes["src"].Value = CanonicalizeUrl(image.Attributes["src"].Value, url);
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
						if (!string.IsNullOrWhiteSpace(href) && System.Text.RegularExpressions.Regex.IsMatch(href, domain))
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