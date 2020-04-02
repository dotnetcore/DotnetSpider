using System;

namespace DotnetSpider.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTimeOffset Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public static DateTimeOffset ToDateTimeOffset(this long timestamp)
        {
            return Epoch.AddMilliseconds(timestamp);
        }

        public static long ToTimestamp(this DateTimeOffset dateTime)
        {
            return (long) (dateTime - Epoch).TotalMilliseconds;
        }
    }
}