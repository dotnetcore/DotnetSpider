using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using DotnetSpider.HtmlAgilityPack.Css;

namespace DotnetSpider.Extraction
{
	/// <summary>
	/// CSS 选择器
	/// </summary>
	public class CssSelector : HtmlSelector
	{
		private readonly string _cssSelector;
		private readonly string _attrName;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="cssSelector">Css 选择器</param>
		public CssSelector(string cssSelector)
		{
			_cssSelector = cssSelector;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="cssSelector">Css 选择器</param>
		/// <param name="attrName">属性名称</param>
		public CssSelector(string cssSelector, string attrName)
		{
			_cssSelector = cssSelector;
			_attrName = attrName;
		}

		/// <summary>
		/// 对节点进行查询, 查询结果为第一个符合查询条件的元素
		/// </summary>
		/// <param name="element">HTML元素</param>
		/// <returns>查询结果</returns>
		public override dynamic Select(HtmlNode element)
		{
			IList<HtmlNode> elements = element.QuerySelectorAll(_cssSelector).ToList();

			if (elements.Count > 0)
			{
				if (string.IsNullOrWhiteSpace(_attrName))
				{
					return elements[0];
				}
				else
				{
					return elements[0].Attributes[_attrName]?.Value?.Trim();
				}
			}
			return null;
		}

		/// <summary>
		/// 对节点进行查询, 查询结果为所有符合查询条件的元素
		/// </summary>
		/// <param name="element">HTML元素</param>
		/// <returns>查询结果</returns>
		public override IEnumerable<dynamic> SelectList(HtmlNode element)
		{
			var els = element.QuerySelectorAll(_cssSelector);
			if (string.IsNullOrWhiteSpace(_attrName))
			{
				return els;
			}
			else
			{
				List<string> result = new List<string>();
				foreach (var el in els)
				{
					var attr = el.Attributes[_attrName];
					if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
					{
						result.Add(attr.Value.Trim());
					}
				}
				return result;
			}
		}

		/// <summary>
		/// 判断查询是否包含属性
		/// </summary>
		/// <returns>如果返回 True, 则说明是查询元素的属性值</returns>
		public override bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);
	}
}
