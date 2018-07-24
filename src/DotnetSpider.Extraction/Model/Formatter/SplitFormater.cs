using System;
using System.Linq;

namespace DotnetSpider.Extraction.Model.Formatter
{
	/// <summary>
	/// Splits a string into substrings based on the strings in an array. You can specify whether the substrings include empty array elements.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SplitFormatter : Formatter
	{
		/// <summary>
		///  A string array that delimits the substrings in this string, an empty array that contains no delimiters, or null.
		/// </summary>
		public string[] Splitor;

		/// <summary>
		/// 分割数值后需要返回的数值索引
		/// </summary>
		public int ElementAt { get; set; } = int.MaxValue;

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override object FormateValue(object value)
		{
			string[] result = value.ToString().Split(Splitor, StringSplitOptions.RemoveEmptyEntries);

			if (result.Length > ElementAt)
			{
				return result[ElementAt];
			}
			else
			{
				return ElementAt == int.MaxValue ? result.Last() : null;
			}
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
			if (Splitor == null || Splitor.Length == 0)
			{
				throw new ArgumentException("Splitors should not be null or empty");
			}

			if (ElementAt < 0)
			{
				throw new ArgumentException("ElementAt should larger than 0");
			}
		}
	}
}
