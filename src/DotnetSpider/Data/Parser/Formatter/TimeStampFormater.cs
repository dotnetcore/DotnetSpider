using System;

namespace DotnetSpider.Data.Parser.Formatter
{
	/// <summary>
	/// 把Unix时间转换成DateTime
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TimeStampFormatter : Formatter
	{
		/// <summary>
		/// 实现数值的转化
		/// </summary>
		/// <param name="value">数值</param>
		/// <returns>被格式化后的数值</returns>
		protected override string FormatValue(string value)
		{
			var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			var tmp = value;
			if (!long.TryParse(tmp, out var timeStamp))
			{
				return dt.ToString("yyyy-MM-dd HH:mm:ss");
			}

			switch (tmp.Length)
			{
				case 10:
					{
						dt = dt.AddSeconds(timeStamp).ToLocalTime();
						break;
					}
				case 13:
					{
						dt = dt.AddMilliseconds(timeStamp).ToLocalTime();
						break;
					}
				default:
					{
						throw new ArgumentException("Wrong input timestamp");
					}
			}
			return dt.ToString("yyyy-MM-dd HH:mm:ss");
		}

		/// <summary>
		/// 校验参数是否设置正确
		/// </summary>
		protected override void CheckArguments()
		{
		}
	}
}
