using System;

namespace DotnetSpider.Infrastructure
{
    public sealed class DateTime2
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
        /// 获取当前Unix时间
        /// </summary>
        /// <returns>Unix时间</returns>
        public static double UnixTimestamp =>
            DateTime.Now.ToUniversalTime().Subtract(Epoch)
                .TotalMilliseconds;

        public static double UnixSecondTimestamp =>
            DateTime.Now.ToUniversalTime().Subtract(Epoch)
                .TotalSeconds;
    }
}