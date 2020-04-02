using System;
using System.Reflection;
using DotnetSpider.Selector;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 属性选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ValueSelector : Selector
	{
		/// <summary>
		/// 属性反射，用于设置解析值到实体对象
		/// </summary>
		internal PropertyInfo PropertyInfo { get; set; }

		/// <summary>
		/// 值是否可以为空, 如果不能为空但解析到的值为空时，当前对象被抛弃
		/// </summary>
		internal bool NotNull { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public ValueSelector()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="type">选择器类型</param>
		/// <param name="expression">表达式</param>
		public ValueSelector(string expression, SelectorType type = SelectorType.XPath)
			: base(expression, type)
		{
		}

		/// <summary>
		/// 数据格式化
		/// </summary>
		public Formatter[] Formatters { get; set; }
	}
}