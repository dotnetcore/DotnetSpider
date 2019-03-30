using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.Selector
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public class Selectable : AbstractSelectable
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="text">内容</param>
		/// <param name="url">URL相对路径补充</param>
		/// <param name="removeOutboundLinks">是否去除外链</param>
		public Selectable(string text, string url, bool removeOutboundLinks = true)
		{
			HtmlDocument document = new HtmlDocument {OptionAutoCloseOnEnd = true};
			document.LoadHtml(text);

			if (!string.IsNullOrWhiteSpace(url))
			{
				FixAllRelativeHref(document, url);
				if (removeOutboundLinks)
				{
					var host = new Uri(url).Host;
					var hostSplits = host.Split('.');
					string domain = $"{hostSplits[hostSplits.Length - 2]}\\.{hostSplits[hostSplits.Length - 1]}";
					RemoveOutboundLinks(document, domain);
				}
			}

			Elements = new List<dynamic> {document.DocumentNode.OuterHtml};
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="text">Json</param>
		public Selectable(string text)
		{
			Elements = new List<dynamic> {text};
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
			return SelectList(Selectors.Css(css));
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
			return SelectList(cssSelector);
		}

		/// <summary>
		/// 通过共用属性查找进村
		/// </summary>
		/// <param name="field">属性名称</param>
		/// <returns>查询结果</returns>
		public override dynamic Environment(string field)
		{
			var key = field.ToLower();
			switch (key)
			{
				case "now":
				{
					return DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
				}
				case "monday":
				{
					var now = DateTime.Now;
					int i = now.DayOfWeek - DayOfWeek.Monday == -1 ? 6 : -1;
					TimeSpan ts = new TimeSpan(i, 0, 0, 0);
					return now.Subtract(ts).Date.ToString("yyyy/MM/dd hh:mm:ss");
				}
				case "today":
				{
					return DateTime.Now.Date.ToString("yyyy/MM/dd hh:mm:ss");
				}
				case "monthly":
				{
					var now = DateTime.Now;
					return now.AddDays(now.Day * -1 + 1).ToString("yyyy/MM/dd hh:mm:ss");
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
				List<dynamic> results = new List<dynamic>();
				foreach (var selectedNode in Elements)
				{
					var result = selector.Select(selectedNode);
					if (result != null)
					{
						results.Add(result);
					}
				}

				return new Selectable(results);
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
				List<dynamic> results = new List<dynamic>();
				foreach (var selectedNode in Elements)
				{
					var result = selector.SelectList(selectedNode);
					if (result != null)
					{
						results.AddRange(result);
					}
				}

				return new Selectable(results);
			}

			throw new ExtractionException($"{nameof(selector)} is null");
		}

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		public override IEnumerable<ISelectable> Nodes()
		{
			List<ISelectable> result = new List<ISelectable>();
			foreach (var element in Elements)
			{
				result.Add(new Selectable(new List<dynamic> {element}));
			}

			return result;
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

		private void FixAllRelativeHref(HtmlDocument document, string url)
		{
			var hrefNodes = document.DocumentNode.SelectNodes(".//@href");
			if (hrefNodes != null)
			{
				foreach (var node in hrefNodes)
				{
					var href = node.Attributes["href"].Value;
					if (!string.IsNullOrWhiteSpace(href) && !href.Contains("http") && !href.Contains("https"))
					{
						node.Attributes["href"].Value = CanonicalizeUrl(href, url);
					}
				}
			}

			var srcNodes = document.DocumentNode.SelectNodes(".//@src");
			if (srcNodes != null)
			{
				foreach (var node in srcNodes)
				{
					var src = node.Attributes["src"].Value;
					if (!string.IsNullOrWhiteSpace(src) && !src.Contains("http") && !src.Contains("https"))
					{
						node.Attributes["src"].Value = CanonicalizeUrl(src, url);
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
						if (!string.IsNullOrWhiteSpace(href) &&
						    System.Text.RegularExpressions.Regex.IsMatch(href, domain))
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