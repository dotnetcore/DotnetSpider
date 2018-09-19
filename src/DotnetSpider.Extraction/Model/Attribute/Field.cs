using System;

namespace DotnetSpider.Extraction.Model.Attribute
{
	/// <summary>
	/// 属性选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class Field : Selector
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public Field()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="type">选择器类型</param>
		/// <param name="expression">表达式</param>
		/// <param name="dataType">数据类型</param>
		/// <param name="length">类型长度</param>
		public Field(string expression, string name, SelectorType type = SelectorType.XPath)
			: base(expression, type)
		{
			Name = name;
		}

		/// <summary>
		/// Define whether the field can be null. 
		/// If set to 'true' and the extractor get no result, the entire class will be discarded.
		/// </summary>
		public bool NotNull { get; set; } = false;

		/// <summary>
		/// 额外选项的定义
		/// </summary>
		public FieldOptions Option { get; set; } = FieldOptions.None;

		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 数据格式化
		/// </summary>
		public Formatter.Formatter[] Formatters { get; set; }

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return Name.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Name} {Expression} {Option}";
		}
	}
}