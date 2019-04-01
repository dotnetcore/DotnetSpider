using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 时间的帮助类
	/// </summary>
	public static class DateTimeHelper
	{
		private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long TicksPerMicrosecond = 10;

		/// <summary>
		/// 当天的RunId: 2017-12-20
		/// </summary>
		public static string TodayString => DateTime.Now.ToString("yyyy-MM-dd");

		/// <summary>
		/// 当月的RunId: 2017-12-01
		/// </summary>
		public static string MonthString => FirstDayOfMonth.ToString("yyyy-MM-dd");


		/// <summary>
		/// 当周的RunId: 2018-01-01 (it's monday)
		/// </summary>
		public static string MondayString => Monday.ToString("yyyy-MM-dd");

		/// <summary>
		/// 当前月份的第一天
		/// </summary>
		public static DateTime FirstDayOfMonth
		{
			get
			{
				var now = DateTime.Now.Date;
				return now.AddDays(now.Day * -1 + 1);
			}
		}

		/// <summary>
		/// 当前月份的最后一天
		/// </summary>
		public static DateTime LastDayOfMonth => FirstDayOfMonth.AddMonths(1).AddDays(-1);


		/// <summary>
		/// 上一月份的第一天
		/// </summary>
		public static DateTime FirstDayOfLastMonth => FirstDayOfMonth.AddMonths(-1);

		/// <summary>
		/// 上一月份的最后一天
		/// </summary>
		public static DateTime LastDayOfLastMonth => FirstDayOfMonth.AddDays(-1);

		/// <summary>
		/// 星期一
		/// </summary>
		public static DateTime Today => DateTime.Now.Date;

		/// <summary>
		/// 星期一
		/// </summary>
		public static DateTime Monday
		{
			get
			{
				var now = DateTime.Now;
				var i = now.DayOfWeek - DayOfWeek.Monday == -1 ? 6 : -1;
				var ts = new TimeSpan(i, 0, 0, 0);

				return now.Subtract(ts).Date;
			}
		}

		/// <summary>
		/// 星期二
		/// </summary>
		public static DateTime Tuesday => Monday.AddDays(1);

		/// <summary>
		/// 星期三
		/// </summary>
		public static DateTime Wednesday => Monday.AddDays(2);

		/// <summary>
		/// 星期四
		/// </summary>
		public static DateTime Thursday => Monday.AddDays(3);

		/// <summary>
		/// 星期五
		/// </summary>
		public static DateTime Friday => Monday.AddDays(4);

		/// <summary>
		/// 星期六
		/// </summary>
		public static DateTime Saturday => Monday.AddDays(5);

		/// <summary>
		/// 星期天
		/// </summary>
		public static DateTime Sunday => Monday.AddDays(6);

		/// <summary>
		/// 把时间转换成Unix时间: 1515133023012
		/// </summary>
		/// <param name="time">时间</param>
		/// <returns>Unix时间</returns>
		public static string ConvertDateTimeToUnix(DateTime time)
		{
			return time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
				.TotalMilliseconds
				.ToString("f0");
		}

		/// <summary>
		/// 把Unix时间转换成DateTime
		/// </summary>
		/// <param name="unixTime">Unix时间</param>
		/// <returns>DateTime</returns>
		public static DateTime ToDateTimeOffset(long unixTime)
		{
			return Epoch.AddTicks(unixTime * TicksPerMicrosecond).DateTime;
		}

		/// <summary>
		/// 获取当前Unix时间
		/// </summary>
		/// <returns>Unix时间</returns>
		public static string GetCurrentUnixTimeString()
		{
			return ConvertDateTimeToUnix(DateTime.UtcNow);
		}

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