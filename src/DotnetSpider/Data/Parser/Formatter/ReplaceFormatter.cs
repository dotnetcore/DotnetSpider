using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ReplaceFormatter : Formatter
	{
		/// <summary>
		/// 需要被替换的值
		/// </summary>
		public string OldValue { get; set; }

		/// <summary>
		/// The string to replace all occurrences of oldValue.
		/// </summary>
		public string NewValue { get; set; }

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			return value.Replace(OldValue, NewValue);
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
