using System;
using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 额外选项的定义
	/// </summary>
	public enum PropertyDefineOptions
	{
		/// <summary>
		/// 不作任何操作
		/// </summary>
		None,

		/// <summary>
		/// For html contene
		/// </summary>
		OuterHtml,

		/// <summary>
		/// For html contene
		/// </summary>
		InnerHtml,

		/// <summary>
		/// For html contene
		/// </summary>
		InnerText,

		/// <summary>
		/// 取的查询器结果的个数作为结果
		/// </summary>
		Count
	}

	/// <summary>
	/// 属性选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyDefine : SelectorAttribute
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public PropertyDefine()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="expression">表达式</param>
		public PropertyDefine(string expression) : base(expression)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="type">选择器类型</param>
		/// <param name="expression">表达式</param>
		public PropertyDefine(SelectorType type, string expression) : base(type, expression)
		{
		}

		/// <summary>
		/// Define whether the field can be null. 
		/// If set to 'true' and the extractor get no result, the entire class will be discarded.
		/// </summary>
		public bool NotNull { get; set; } = false;

		/// <summary>
		/// 额外选项的定义
		/// </summary>
		public PropertyDefineOptions Option { get; set; } = PropertyDefineOptions.None;

		/// <summary>
		/// 列的长度
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// 是否不把此列数据保存到数据库
		/// </summary>
		public bool IgnoreStore { get; set; }
	}
}