using System;

namespace DotnetSpider.Proxy
{
	public static class DateTimeUtil
	{
		/// <summary>
		/// 获取当前Unix时间
		/// </summary>
		/// <returns>Unix时间</returns>
		public static double GetCurrentUnixTimeNumber()
		{
			return DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
				.TotalMilliseconds;
		}
	}
}
