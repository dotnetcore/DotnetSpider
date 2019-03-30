using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 如果值等于EqualValue, 则返回Displacement. 比如用于: 采集的结果为: 是, 转化为 False
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DisplaceFormatter : Formatter
	{
		/// <summary>
		/// 比较的值
		/// </summary>
		public string EqualValue { get; set; }

		/// <summary>
		/// 最终替换的值
		/// </summary>
		public string Displacement { get; set; }

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			return value.Equals(EqualValue) ? Displacement : value;
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
