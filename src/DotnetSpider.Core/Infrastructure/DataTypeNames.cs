using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class DataTypeNames
	{
		public static readonly string Int = typeof(int).FullName;
		public static readonly string Int64 = typeof(long).FullName;
		public static readonly string Double = typeof(double).FullName;
		public static readonly string String = typeof(string).FullName;
		public static readonly string Float = typeof(float).FullName;
		public static readonly string DateTime = typeof(DateTime).FullName;
		public static readonly string Boolean = typeof(bool).FullName;
		public static readonly string Decimal = typeof(decimal).FullName;
		public static readonly string TimeUuid = "Cassandra.TimeUuid";
	}
}
