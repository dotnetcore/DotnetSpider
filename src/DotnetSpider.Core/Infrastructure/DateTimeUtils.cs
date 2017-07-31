using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class DateTimeUtils
	{
		private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long TicksPerMicrosecond = 10;

		public static string ConvertDateTimeToUnix(DateTime time)
		{
			return time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString("f0");
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

			First_Day_Of_Current_Month = now.AddDays(now.Day * -1 + 1);
			Last_Day_Of_Current_Month = First_Day_Of_Current_Month.AddMonths(1).AddDays(-1);
			First_Day_Of_Previous_Month = First_Day_Of_Current_Month.AddMonths(-1);
			Last_Day_Of_Previous_Month = First_Day_Of_Current_Month.AddDays(-1);

			int i = now.DayOfWeek - DayOfWeek.Monday;
			if (i == -1)
			{
				// i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
				i = 6;
			}
			TimeSpan ts = new TimeSpan(i, 0, 0, 0);

			Monday_Of_Current_Week = now.Subtract(ts).Date;
			Tuesday_Of_Current_Week = Monday_Of_Current_Week.AddDays(1);
			Wednesday_Of_Current_Week = Monday_Of_Current_Week.AddDays(2);
			Thursday_Of_Current_Week = Monday_Of_Current_Week.AddDays(3);
			Friday_Of_Current_Week = Monday_Of_Current_Week.AddDays(4);
			Saturday_Of_Current_Week = Monday_Of_Current_Week.AddDays(5);
			Sunday_Of_Current_Week = Monday_Of_Current_Week.AddDays(6);

			Monday_Of_Previous_Week = Monday_Of_Current_Week.AddDays(-7);
			Tuesday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(1);
			Wednesday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(2);
			Thursday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(3);
			Friday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(4);
			Saturday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(5);
			Sunday_Of_Previous_Week = Monday_Of_Previous_Week.AddDays(6);

			Monday_Of_Next_Week = Sunday_Of_Current_Week.AddDays(1);
			Tuesday_Of_Next_Week = Monday_Of_Next_Week.AddDays(1);
			Wednesday_Of_Next_Week = Monday_Of_Next_Week.AddDays(2);
			Thursday_Of_Next_Week = Monday_Of_Next_Week.AddDays(3);
			Friday_Of_Next_Week = Monday_Of_Next_Week.AddDays(4);
			Saturday_Of_Next_Week = Monday_Of_Next_Week.AddDays(5);
			Sunday_Of_Next_Week = Monday_Of_Next_Week.AddDays(6);

			RunIdOfToday = now.ToString("yyyy-MM-dd");
			RunIdOfMonthly = First_Day_Of_Current_Month.ToString("yyyy-MM-dd");
			RunIdOfMonday = Monday_Of_Current_Week.ToString("yyyy-MM-dd");
		}

		public static DateTime First_Day_Of_Current_Month { get; }
		public static DateTime Last_Day_Of_Current_Month { get; }
		public static DateTime First_Day_Of_Previous_Month { get; }
		public static DateTime Last_Day_Of_Previous_Month { get; }
		public static DateTime Monday_Of_Current_Week { get; }
		public static DateTime Tuesday_Of_Current_Week { get; }
		public static DateTime Wednesday_Of_Current_Week { get; }
		public static DateTime Thursday_Of_Current_Week { get; }
		public static DateTime Friday_Of_Current_Week { get; }
		public static DateTime Saturday_Of_Current_Week { get; }
		public static DateTime Sunday_Of_Current_Week { get; }
		public static DateTime Monday_Of_Previous_Week { get; }
		public static DateTime Tuesday_Of_Previous_Week { get; }
		public static DateTime Wednesday_Of_Previous_Week { get; }
		public static DateTime Thursday_Of_Previous_Week { get; }
		public static DateTime Friday_Of_Previous_Week { get; }
		public static DateTime Saturday_Of_Previous_Week { get; }
		public static DateTime Sunday_Of_Previous_Week { get; }
		public static DateTime Monday_Of_Next_Week { get; }
		public static DateTime Tuesday_Of_Next_Week { get; }
		public static DateTime Wednesday_Of_Next_Week { get; }
		public static DateTime Thursday_Of_Next_Week { get; }
		public static DateTime Friday_Of_Next_Week { get; }
		public static DateTime Saturday_Of_Next_Week { get; }
		public static DateTime Sunday_Of_Next_Week { get; }

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