using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// Replaces one or more format items in a specified string with the string representation of a specified object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class StringFormatter : Formatter
	{
		/// <summary>
		/// A composite format string.
		/// </summary>
		public string FormatStr { get; set; }

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			return string.Format(FormatStr, value);
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
			if (string.IsNullOrWhiteSpace(FormatStr))
			{
				throw new ArgumentException("FormatString should not be null or empty");
			}
		}
	}
}
