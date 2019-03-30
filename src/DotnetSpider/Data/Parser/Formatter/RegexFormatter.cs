using System;
using System.Text.RegularExpressions;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 如果能匹配正则表达式则返回True的内容, 如果不符合正则表达式则返回 False的内容
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexFormatter : Formatter
	{
		private const string Id = "227a207a28024b1cbee3754e76443df2";

		/// <summary>
		/// 正则表达式格式化
		/// </summary>
		public string Pattern { get; set; }

		/// <summary>
		/// 符合正则表达式应该返回的内容
		/// </summary>
		public string True { get; set; } = Id;

		/// <summary>
		/// 不符合正则表达式应该返回的内容
		/// </summary>
		public string False { get; set; } = Id;

		/// <summary>
		/// 如果 True没有设值, 则返回正则表达式匹配的 Group 内容
		/// </summary>
		public int Group { get; set; } = -1;

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>

		protected override string FormatValue(string value)
		{
			var tmp = value;
			var matches = Regex.Matches(tmp, Pattern);
			if (matches.Count > 0)
			{
				if (True == Id)
				{
					return Group < 0 ? matches[0].Value : matches[0].Groups[Group].Value;
				}

				return True;
			}

			return False == Id ? "" : False;
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
