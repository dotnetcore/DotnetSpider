using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// 查询接口
	/// </summary>
	public interface ISelectable
	{
		/// <summary>
		/// 通过XPath查找结果
		/// </summary>
		/// <param name="xpath">XPath 表达式</param>
		/// <returns>查询接口</returns>
		ISelectable XPath(string xpath);

		/// <summary>
		/// 通过Css 选择器查找结果
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <returns>查询接口</returns>
		ISelectable Css(string css);

		/// <summary>
		/// 通过Css 选择器查找元素, 并取得属性的值
		/// </summary>
		/// <param name="css">Css 选择器</param>
		/// <param name="attrName">查询到的元素的属性</param>
		/// <returns>查询接口</returns>
		ISelectable Css(string css, string attrName);

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		ISelectable Links();

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		IList<ISelectable> Nodes();

		/// <summary>
		/// 通过JsonPath查找结果
		/// </summary>
		/// <param name="jsonPath">JsonPath 表达式</param>
		/// <returns>查询接口</returns>
		ISelectable JsonPath(string jsonPath);

		/// <summary>
		/// 通过正则表达式查找结果
		/// </summary>
		/// <param name="regex">正则表达式</param>
		/// <returns>查询接口</returns>
		ISelectable Regex(string regex);

		/// <summary>
		/// 通过正则表达式查找结果
		/// </summary>
		/// <param name="regex">正则表达式</param>
		/// <param name="group">分组</param>
		/// <returns>查询接口</returns>
		ISelectable Regex(string regex, int group);

		/// <summary>
		/// 获得当前查询器的文本结果, 如果查询结果为多个, 则返回第一个结果的值
		/// </summary>
		/// <param name="isPlainText">是否纯文本化、去掉HTML标签</param>
		/// <returns>查询到的文本结果</returns>
		string GetValue(bool isPlainText = false);

		/// <summary>
		/// 获得当前查询器的文本结果
		/// </summary>
		/// <param name="isPlainText">是否纯文本化、去掉HTML标签</param>
		/// <returns>查询到的文本结果</returns>
		List<string> GetValues(bool isPlainText = false);

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		ISelectable Select(ISelector selector);

		/// <summary>
		/// 通过查询器查找结果
		/// </summary>
		/// <param name="selector">查询器</param>
		/// <returns>查询接口</returns>
		ISelectable SelectList(ISelector selector);
	}
}
