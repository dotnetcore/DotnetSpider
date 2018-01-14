using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model.Formatter
{
	/// <summary>
	/// 把包含中文的字符串转化成数字
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DigitUnitFormater : Formatter
	{
		private const string _unitStringForShi = "十";
		private const string _unitStringForBai = "百";
		private const string _unitStringForQian = "千";
		private const string _unitStringForWan = "万";
		private const string _unitStringForYi = "亿";

		/// <summary>
		/// 数字格式化模版
		/// </summary>
		public string NumberFormat { get; set; } = "F0";

		/// <summary>
		/// 把包含中文的字符串转化成数字
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override object FormateValue(object value)
		{
			var tmp = value.ToString();
			decimal num = decimal.Parse(RegexUtil.Decimal.Match(tmp).ToString());
			if (tmp.EndsWith(_unitStringForShi))
			{
				num = num * 10;
			}
			else if (tmp.EndsWith(_unitStringForBai))
			{
				num = num * 100;
			}
			else if (tmp.EndsWith(_unitStringForBai))
			{
				num = num * 100;
			}
			else if (tmp.EndsWith(_unitStringForQian))
			{
				num = num * 1000;
			}
			else if (tmp.EndsWith(_unitStringForWan))
			{
				num = num * 10000;
			}
			else if (tmp.EndsWith(_unitStringForYi))
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
