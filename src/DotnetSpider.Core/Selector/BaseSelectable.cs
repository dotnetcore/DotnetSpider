using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public abstract class BaseSelectable : ISelectable
	{
		/// <summary>
		/// 查找到的所有结果
		/// </summary>
		public List<dynamic> Elements { get; set; }

		/// <summary>
		/// 通过XPath查找结果
		/// </summary>
		/// <param name="xpath">XPath 表达式</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable XPath(string xpath);

		/// <summary>
		/// 通过Css 选择器查找结果
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable Css(string css);

		/// <summary>
		/// 通过Css 选择器查找元素, 并取得属性的值
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <param name="attrName">查询到的元素的属性</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable Css(string css, string attrName);

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		public abstract ISelectable Links();

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		public abstract IList<ISelectable> Nodes();

		/// <summary>
		/// 通过JsonPath查找结果
		/// </summary>
		/// <param name="jsonPath">JsonPath 表达式</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable JsonPath(string jsonPath);

		/// <summary>
		/// 通过正则表达式查找结果
		/// </summary>
		/// <param name="regex">正则表达式</param>
		/// <returns>查询接口</returns>
		public ISelectable Regex(string regex)
		{
			return Select(Selectors.Regex(regex));
		}

		/// <summary>
		/// 通过正则表达式查找结果
		/// </summary>
		/// <param name="regex">正则表达式</param>
		/// <param name="group">分组</param>
		/// <returns>查询接口</returns>
		public ISelectable Regex(string regex, int group)
		{
			return Select(Selectors.Regex(regex, group));
		}

		/// <summary>
		/// 获得当前查询器的文本结果, 如果查询结果为多个, 则返回第一个结果的值
		/// </summary>
		/// <param name="isPlainText">是否纯文本化、去掉HTML标签</param>
		/// <returns>查询到的文本结果</returns>
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

		/// <summary>
		/// 获得当前查询器的文本结果
		/// </summary>
		/// <param name="isPlainText">是否纯文本化、去掉HTML标签</param>
		/// <returns>查询到的文本结果</returns>
		public List<string> GetValues(bool isPlainText)
		{
			List<string> result = new List<string>();
			foreach (var el in Elements)
			{
				if (el is HtmlNode node)
				{
					result.Add(!isPlainText ? node.InnerHtml : node.InnerText.Trim());
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

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable Select(ISelector selector);

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		public abstract ISelectable SelectList(ISelector selector);

	}
}
