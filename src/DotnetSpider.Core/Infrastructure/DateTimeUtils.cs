using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class DateTimeUtils
	{
		private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long TicksPerMicrosecond = 10;

		public static string ConvertDateTimeToUnix(DateTime time)
		{
			return time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0");
		}

		/// <returns></returns>
		public static string GetCurrentTimeStampString()
		{
			return ConvertDateTimeToUnix(DateTime.UtcNow);
		}

		/// <returns></returns>
		public static double GetCurrentTimeStamp()
		{
			return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}

		public static string RunIdOfToday { get; }
		public static string RunIdOfMonthly { get; }
		public static string RunIdOfMonday { get; }

		static DateTimeUtils()
		{
			var now = DateTime.Now.Date;

			FirstDayOfCurrentMonth = now.AddDays(now.Day * -1 + 1);
			LastDayOfCurrentMonth = FirstDayOfCurrentMonth.AddMonths(1).AddDays(-1);
			FirstDayOfPreviousMonth = FirstDayOfCurrentMonth.AddMonths(-1);
			LastDayOfPreviousMonth = FirstDayOfCurrentMonth.AddDays(-1);

			int i = now.DayOfWeek - DayOfWeek.Monday;
			if (i == -1)
			{
				// i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
				i = 6;
			}
			TimeSpan ts = new TimeSpan(i, 0, 0, 0);

			MondayOfCurrentWeek = now.Subtract(ts).Date;
			TuesdayOfCurrentWeek = MondayOfCurrentWeek.AddDays(1);
			WednesdayOfCurrentWeek = MondayOfCurrentWeek.AddDays(2);
			ThursdayOfCurrentWeek = MondayOfCurrentWeek.AddDays(3);
			FridayOfCurrentWeek = MondayOfCurrentWeek.AddDays(4);
			SaturdayOfCurrentWeek = MondayOfCurrentWeek.AddDays(5);
			SundayOfCurrentWeek = MondayOfCurrentWeek.AddDays(6);

			MondayOfPreviousWeek = MondayOfCurrentWeek.AddDays(-7);
			TuesdayOfPreviousWeek = MondayOfPreviousWeek.AddDays(1);
			WednesdayOfPreviousWeek = MondayOfPreviousWeek.AddDays(2);
			ThursdayOfPreviousWeek = MondayOfPreviousWeek.AddDays(3);
			FridayOfPreviousWeek = MondayOfPreviousWeek.AddDays(4);
			SaturdayOfPreviousWeek = MondayOfPreviousWeek.AddDays(5);
			SundayOfPreviousWeek = MondayOfPreviousWeek.AddDays(6);

			MondayOfNextWeek = SundayOfCurrentWeek.AddDays(1);
			TuesdayOfNextWeek = MondayOfNextWeek.AddDays(1);
			WednesdayOfNextWeek = MondayOfNextWeek.AddDays(2);
			ThursdayOfNextWeek = MondayOfNextWeek.AddDays(3);
			FridayOfNextWeek = MondayOfNextWeek.AddDays(4);
			SaturdayOfNextWeek = MondayOfNextWeek.AddDays(5);
			SundayOfNextWeek = MondayOfNextWeek.AddDays(6);

			RunIdOfToday = now.ToString("yyyy-MM-dd");
			RunIdOfMonthly = FirstDayOfCurrentMonth.ToString("yyyy-MM-dd");
			RunIdOfMonday = MondayOfCurrentWeek.ToString("yyyy-MM-dd");
		}

		public static DateTime FirstDayOfCurrentMonth { get; }
		public static DateTime LastDayOfCurrentMonth { get; }
		public static DateTime FirstDayOfPreviousMonth { get; }
		public static DateTime LastDayOfPreviousMonth { get; }
		public static DateTime MondayOfCurrentWeek { get; }
		public static DateTime TuesdayOfCurrentWeek { get; }
		public static DateTime WednesdayOfCurrentWeek { get; }
		public static DateTime ThursdayOfCurrentWeek { get; }
		public static DateTime FridayOfCurrentWeek { get; }
		public static DateTime SaturdayOfCurrentWeek { get; }
		public static DateTime SundayOfCurrentWeek { get; }
		public static DateTime MondayOfPreviousWeek { get; }
		public static DateTime TuesdayOfPreviousWeek { get; }
		public static DateTime WednesdayOfPreviousWeek { get; }
		public static DateTime ThursdayOfPreviousWeek { get; }
		public static DateTime FridayOfPreviousWeek { get; }
		public static DateTime SaturdayOfPreviousWeek { get; }
		public static DateTime SundayOfPreviousWeek { get; }
		public static DateTime MondayOfNextWeek { get; }
		public static DateTime TuesdayOfNextWeek { get; }
		public static DateTime WednesdayOfNextWeek { get; }
		public static DateTime ThursdayOfNextWeek { get; }
		public static DateTime FridayOfNextWeek { get; }
		public static DateTime SaturdayOfNextWeek { get; }
		public static DateTime SundayOfNextWeek { get; }

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