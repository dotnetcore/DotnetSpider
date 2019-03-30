using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 截取数值
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CutoutFormatter : Formatter
	{
		/// <summary>
		/// 起始部分的内容
		/// </summary>
		public string StartPart { get; set; }

		/// <summary>
		/// 结束部分的内容
		/// </summary>
		public string EndPart { get; set; }

		/// <summary>
		/// 开始截取的偏移
		/// </summary>
		public int StartOffset { get; set; } = 0;

		/// <summary>
		/// 结束截取的偏移
		/// </summary>
		public int EndOffset { get; set; } = 0;

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			var tmp = value;
			int begin = tmp.IndexOf(StartPart, StringComparison.Ordinal);
			int length;
			if (!string.IsNullOrEmpty(EndPart))
			{
				int end = tmp.IndexOf(EndPart, begin, StringComparison.Ordinal);
				length = end - begin;
			}
			else
			{
				length = tmp.Length - begin;
			}

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			if (!string.IsNullOrEmpty(EndPart))
			{
				length += EndPart.Length;
			}

			return tmp.Substring(begin, length);
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}