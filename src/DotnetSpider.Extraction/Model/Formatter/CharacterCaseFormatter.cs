using System;

namespace DotnetSpider.Extraction.Model.Formatter
{
	/// <summary>
	/// 字符串大写化或者小写化
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class CharacterCaseFormatter : Formatter
	{
		/// <summary>
		/// 如果为 True 则把数据大写化, 如果为 False 则表数据小写化
		/// </summary>
		public bool ToUpper { get; set; } = true;

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override object FormatValue(object value)
		{
			return ToUpper ? value.ToString().ToUpperInvariant() : value.ToString().ToLowerInvariant();
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
