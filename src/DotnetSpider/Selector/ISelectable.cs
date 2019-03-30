using System.Collections.Generic;

namespace DotnetSpider.Selector
{
	/// <summary>
	/// 元素取值方式
	/// </summary>
	public enum ValueOption
	{
		/// <summary>
		/// For json content
		/// </summary>
		None,

		/// <summary>
		/// For html content
		/// </summary>
		OuterHtml,

		/// <summary>
		/// For html content
		/// </summary>
		InnerHtml,

		/// <summary>
		/// For html content
		/// </summary>
		InnerText,
		
		Count
	}

	/// <summary>
	/// 查询接口
	/// </summary>
	public interface ISelectable
	{
		Dictionary<string, dynamic> Properties { get; set; }

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
		/// <param name="attr">查询到的元素的属性</param>
		/// <returns>查询接口</returns>
		ISelectable Css(string css, string attr);

		/// <summary>
		/// 通过共用属性查找进村
		/// </summary>
		/// <param name="field">属性名称</param>
		/// <returns>查询结果</returns>
		dynamic Environment(string field);

		/// <summary>
		/// 查找所有的链接
		/// </summary>
		/// <returns>查询接口</returns>
		ISelectable Links();

		/// <summary>
		/// 取得查询器里所有的结果
		/// </summary>
		/// <returns>查询接口</returns>
		IEnumerable<ISelectable> Nodes();

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
		/// <param name="option">元素取值方式</param>
		/// <returns>查询到的文本结果</returns>
		string GetValue(ValueOption option = ValueOption.None);

		/// <summary>
		/// 获得当前查询器的文本结果
		/// </summary>
		/// <param name="option">元素取值方式</param>
		/// <returns>查询到的文本结果</returns>
		IEnumerable<string> GetValues(ValueOption option = ValueOption.None);

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
