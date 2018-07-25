using Newtonsoft.Json;
using System;

namespace DotnetSpider.Extraction.Model.Attribute
{
	/// <summary>
	/// 属性选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class FieldSelector : Selector
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public FieldSelector()
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
		public FieldSelector(string expression, string name, SelectorType type = SelectorType.XPath, DataType dataType = DataType.String, int length = 255)
			: base(expression, type)
		{
			Name = name;
			DataType = dataType;
			Length = length;
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
		/// 列的长度
		/// </summary>
		public int Length { get; set; } = 255;

		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 是否不把此列数据保存到数据库
		/// </summary>
		public bool IgnoreStore { get; set; }

		/// <summary>
		/// 数据类型
		/// </summary>
		public DataType DataType { get; set; } = DataType.None;

		/// <summary>
		/// 是否是主键
		/// </summary>
		public bool IsPrimary { get; set; }

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
			return $"{Name} {DataType} {IgnoreStore}";
		}
	}
}