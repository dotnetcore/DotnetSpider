using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// Removes all leading and trailing white-space characters from the current System.String object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TrimFormatter : Formatter
	{
		/// <summary>
		/// Trim 类型
		/// </summary>
		public enum TrimType
		{
			/// <summary>
			/// 只Trim后边
			/// </summary>
			Right,
			/// <summary>
			/// 只Trim前边
			/// </summary>
			Left,

			/// <summary>
			/// Trim前后
			/// </summary>
			All
		}

		/// <summary>
		/// Trim 类型
		/// </summary>
		public TrimType Type { get; set; } = TrimType.All;

		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			switch (Type)
			{
				case TrimType.All:
					{
						return value.Trim();
					}
				case TrimType.Left:
					{
						return value.TrimStart();
					}
				case TrimType.Right:
					{
						return value.TrimEnd();
					}
				default:
					{
						return value.Trim();
					}
			}
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
