using System;
using System.Text.RegularExpressions;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 把包含中文的字符串转化成数字
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DigitUnitFormatter : Formatter
	{
		private const string UnitStringForShi = "十";
		private const string UnitStringForBai = "百";
		private const string UnitStringForQian = "千";
		private const string UnitStringForWan = "万";
		private const string UnitStringForYi = "亿";
		private readonly Regex _decimalRegex = new Regex(@"\d+(\.\d+)?");

		/// <summary>
		/// 数字格式化模版
		/// </summary>
		public string NumberFormat { get; set; } = "F0";

		/// <summary>
		/// 把包含中文的字符串转化成数字
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			var tmp = value;
			var num = decimal.Parse(_decimalRegex.Match(tmp).ToString());
			if (tmp.EndsWith(UnitStringForShi))
			{
				num = num * 10;
			}
			else if (tmp.EndsWith(UnitStringForBai))
			{
				num = num * 100;
			}
			else if (tmp.EndsWith(UnitStringForBai))
			{
				num = num * 100;
			}
			else if (tmp.EndsWith(UnitStringForQian))
			{
				num = num * 1000;
			}
			else if (tmp.EndsWith(UnitStringForWan))
			{
				num = num * 10000;
			}
			else if (tmp.EndsWith(UnitStringForYi))
			{
				num = num * 100000000;
			}
			return num.ToString(NumberFormat);
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
