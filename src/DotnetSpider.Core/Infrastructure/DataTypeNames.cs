using System;

namespace DotnetSpider.Core.Infrastructure
{
	public class DataTypeNames
	{
		public static string Int = typeof(int).FullName;
		public static string Int64 = typeof(long).FullName;
		public static string Double = typeof(double).FullName;
		public static string String = typeof(string).FullName;
		public static string Float = typeof(float).FullName;
		public static string DateTime = typeof(DateTime).FullName;
		public static string Boolean = typeof(bool).FullName;
		public static string Decimal = typeof(decimal).FullName;
		public static string TimeUuid = "Cassandra.TimeUuid";
	}
}
