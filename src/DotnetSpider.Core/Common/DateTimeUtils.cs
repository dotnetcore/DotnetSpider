using System;

namespace DotnetSpider.Core.Common
{
	public static class DateTimeUtils
	{
		/// <returns></returns>
		public static string GetCurrentTimeStampString()
		{
			return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0");
		}

		/// <returns></returns>
		public static double GetCurrentTimeStamp()
		{
			return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}

		private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long TicksPerMicrosecond = 10;

		public static string TodayRunId;
		public static string MonthlyRunId;
		public static string MondayRunId;

		static DateTimeUtils()
		{
			var now = DateTime.Now.Date;

			FirstDayofThisMonth = now.AddDays(now.Day * -1 + 1);

			LastDayofThisMonth = FirstDayofThisMonth.AddMonths(1).AddDays(-1);

			FirstDayofLastMonth = FirstDayofThisMonth.AddMonths(-1);

			LastDayofLastMonth = FirstDayofThisMonth.AddDays(-1);

			FirstDayofThisWeek = now.AddDays(Convert.ToInt32(now.DayOfWeek.ToString("d")) * -1);

			FirstDayofLastWeek = FirstDayofThisWeek.AddDays(-7);

			LastDayofThisWeek = FirstDayofThisWeek.AddDays(6);

			LastDayofLastWeek = FirstDayofThisWeek.AddDays(-1);

			TodayRunId = DateTime.Now.ToString("yyyy-MM-dd");
			MonthlyRunId = FirstDayofThisMonth.ToString("yyyy-MM");
			MondayRunId = FirstDayofThisWeek.ToString("yyyy-MM-dd");
		}

		public static DateTime FirstDayofThisMonth { get; }
		public static DateTime LastDayofThisMonth { get; private set; }
		public static DateTime FirstDayofLastMonth { get; private set; }
		public static DateTime LastDayofLastMonth { get; private set; }
		public static DateTime FirstDayofLastWeek { get; private set; }
		public static DateTime FirstDayofThisWeek { get; }
		public static DateTime LastDayofThisWeek { get; private set; }
		public static DateTime LastDayofLastWeek { get; private set; }

		public static DateTime GetFirstDayofMoth(DateTime selectDate)
		{
			return selectDate.AddDays(selectDate.Day * -1 + 1).Date;
		}

		/// <summary>
		/// Returns the number of microseconds since Epoch of the current UTC date and time.
		/// </summary>
		public static long UtcNow => FromDateTimeOffset(DateTimeOffset.UtcNow);

		/// <summary>
		/// Converts the microseconds since Epoch time provided to a DateTimeOffset.
		/// </summary>
		public static DateTimeOffset ToDateTimeOffset(long microsecondsSinceEpoch)
		{
			return Epoch.AddTicks(microsecondsSinceEpoch * TicksPerMicrosecond);
		}

		/// <summary>
		/// Converts the DateTimeOffset provided to the number of microseconds since Epoch.
		/// </summary>
		public static long FromDateTimeOffset(DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.Subtract(Epoch).Ticks / TicksPerMicrosecond;
		}

		/// <summary>
		/// Converts the DateTimeOffset provided to the number of microseconds since Epoch.
		/// </summary>
		public static long ToMicrosecondsSinceEpoch(this DateTimeOffset dateTimeOffset)
		{
			return FromDateTimeOffset(dateTimeOffset);
		}
	}
}