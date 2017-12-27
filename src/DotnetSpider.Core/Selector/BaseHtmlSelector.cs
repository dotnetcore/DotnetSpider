using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// HTML文件查询的抽象
	/// </summary>
	public abstract class BaseHtmlSelector : ISelector
	{
		/// <summary>
		/// 判断查询是否包含属性
		/// </summary>
		/// <returns></returns>
		public abstract bool HasAttribute();

		/// <summary>
		/// 对节点进行查询, 查询结果为第一个符合查询条件的元素
		/// </summary>
		/// <param name="element"><see cref="HtmlNode"/></param>
		/// <returns>查询结果</returns>
		public abstract dynamic Select(HtmlNode element);

		/// <summary>
		/// 对节点进行查询, 查询结果为所有符合查询条件的元素
		/// </summary>
		/// <param name="element"><see cref="HtmlNode"/></param>
		/// <returns>查询结果</returns>
		public abstract List<dynamic> SelectList(HtmlNode element);

		/// <summary>
		/// 对Html文本进行查询, 查询结果为第一个符合查询条件的元素
		/// </summary>
		/// <param name="text">Html文本</param>
		/// <returns>查询结果</returns>
		public virtual dynamic Select(dynamic text)
		{
			if (text != null)
			{
				if (text is string)
				{
					HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
					document.LoadHtml(text);
					return Select(document.DocumentNode);
				}
				else
				{
					return Select(text as HtmlNode);
				}
			}
			return null;
		}

		/// <summary>
		/// 对Html文本进行查询, 查询结果为所有符合查询条件的元素
		/// </summary>
		/// <param name="text">Html文本</param>
		/// <returns>查询结果</returns>
		public virtual List<dynamic> SelectList(dynamic text)
		{
			if (text != null)
			{
				if (text is HtmlNode htmlNode)
				{
					return SelectList(htmlNode);
				}
				else
				{
					HtmlDocument document = new HtmlDocument { OptionAutoCloseOnEnd = true };
					document.LoadHtml(text);
					return SelectList(document.DocumentNode);
				}
			}
			else
			{
				return new List<dynamic>();
			}
		}
	}
}