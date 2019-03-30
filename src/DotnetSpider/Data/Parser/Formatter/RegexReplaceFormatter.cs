using System;
using System.Text.RegularExpressions;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	///  In a specified input string, replaces all strings that match a specified regular expression with a specified replacement string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexReplaceFormatter : Formatter
	{
		/// <summary>
		/// 正则表达式
		/// </summary>
		public string Pattern { get; set; }

		/// <summary>
		/// The replacement string
		/// </summary>
		public string NewValue{ get; set; }

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			return Regex.Replace(value, Pattern, NewValue);
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
			if (string.IsNullOrWhiteSpace(Pattern))
			{
				throw new ArgumentException("Pattern should not be null or empty");
			}
		}
	}
}
