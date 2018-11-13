using System;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property)]
	public class Column : Attribute
	{
		/// <summary>
		/// 列的长度
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// 列名
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 数据类型, 如果不设置系统会从参考属性的类型
		/// </summary>
		public DataType DataType { get; set; }

		public Column(int length = 255)
		{
			Length = length;
		}

		public Column(string name, int length = 255) : this(length)
		{
			Name = name;
		}

		public override int GetHashCode()
		{
			return string.IsNullOrWhiteSpace(Name) ? -1 : Name.GetHashCode();
		}
	}
}
