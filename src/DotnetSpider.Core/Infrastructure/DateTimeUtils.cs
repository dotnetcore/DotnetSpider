using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class DateTimeUtils
	{
		private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long TicksPerMicrosecond = 10;

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

		public static string RunIdOfToday { get; }
		public static string RunIdOfMonthly { get; }
		public static string RunIdOfMonday { get; }

		static DateTimeUtils()
		{
			var now = DateTime.Now.Date;

			Day1OfThisMonth = now.AddDays(now.Day * -1 + 1);
			LastDayOfThisMonth = Day1OfThisMonth.AddMonths(1).AddDays(-1);
			Day1OfLastMonth = Day1OfThisMonth.AddMonths(-1);
			LastDayOfLastMonth = Day1OfThisMonth.AddDays(-1);

			int i = now.DayOfWeek - DayOfWeek.Monday;
			if (i == -1)
			{
				// i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
				i = 6;
			}
			TimeSpan ts = new TimeSpan(i, 0, 0, 0);

			Day1OfThisWeek = now.Subtract(ts).Date;
			Day2OfThisWeek = Day1OfThisWeek.AddDays(1);
			Day3OfThisWeek = Day1OfThisWeek.AddDays(2);
			Day4OfThisWeek = Day1OfThisWeek.AddDays(3);
			Day5OfThisWeek = Day1OfThisWeek.AddDays(4);
			Day6OfThisWeek = Day1OfThisWeek.AddDays(5);
			Day7OfThisWeek = Day1OfThisWeek.AddDays(6);

			Day1OfLastWeek = Day1OfThisWeek.AddDays(-7);
			Day2OfLastWeek = Day1OfLastWeek.AddDays(1);
			Day3OfLastWeek = Day1OfLastWeek.AddDays(2);
			Day4OfLastWeek = Day1OfLastWeek.AddDays(3);
			Day5OfLastWeek = Day1OfLastWeek.AddDays(4);
			Day6OfLastWeek = Day1OfLastWeek.AddDays(5);
			Day7OfLastWeek = Day1OfLastWeek.AddDays(6);

			Day1OfNextWeek = Day7OfThisWeek.AddDays(1);
			Day2OfNextWeek = Day1OfNextWeek.AddDays(1);
			Day3OfNextWeek = Day1OfNextWeek.AddDays(2);
			Day4OfNextWeek = Day1OfNextWeek.AddDays(3);
			Day5OfNextWeek = Day1OfNextWeek.AddDays(4);
			Day6OfNextWeek = Day1OfNextWeek.AddDays(5);
			Day7OfNextWeek = Day1OfNextWeek.AddDays(6);

			RunIdOfToday = now.ToString("yyyy_MM_dd");
			RunIdOfMonthly = Day1OfThisMonth.ToString("yyyy_MM");
			RunIdOfMonday = Day1OfThisWeek.ToString("yyyy_MM_dd");
		}

		public static DateTime Day1OfThisMonth { get; }
		public static DateTime LastDayOfThisMonth { get; }
		public static DateTime Day1OfLastMonth { get; }
		public static DateTime LastDayOfLastMonth { get; }
		public static DateTime Day1OfThisWeek { get; }
		public static DateTime Day2OfThisWeek { get; }
		public static DateTime Day3OfThisWeek { get; }
		public static DateTime Day4OfThisWeek { get; }
		public static DateTime Day5OfThisWeek { get; }
		public static DateTime Day6OfThisWeek { get; }
		public static DateTime Day7OfThisWeek { get; }
		public static DateTime Day1OfLastWeek { get; }
		public static DateTime Day2OfLastWeek { get; }
		public static DateTime Day3OfLastWeek { get; }
		public static DateTime Day4OfLastWeek { get; }
		public static DateTime Day5OfLastWeek { get; }
		public static DateTime Day6OfLastWeek { get; }
		public static DateTime Day7OfLastWeek { get; }
		public static DateTime Day1OfNextWeek { get; }
		public static DateTime Day2OfNextWeek { get; }
		public static DateTime Day3OfNextWeek { get; }
		public static DateTime Day4OfNextWeek { get; }
		public static DateTime Day5OfNextWeek { get; }
		public static DateTime Day6OfNextWeek { get; }
		public static DateTime Day7OfNextWeek { get; }

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