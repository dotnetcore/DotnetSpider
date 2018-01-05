using System;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 数据类型的名称
	/// </summary>
	public static class DataTypeNames
	{
		/// <summary>
		/// Int 的类型全称
		/// </summary>
		public static readonly string Int = typeof(int).FullName;
		/// <summary>
		/// Int64 的类型全称
		/// </summary>
		public static readonly string Int64 = typeof(long).FullName;
		/// <summary>
		/// Double 的类型全称
		/// </summary>
		public static readonly string Double = typeof(double).FullName;
		/// <summary>
		/// String 的类型全称
		/// </summary>
		public static readonly string String = typeof(string).FullName;
		/// <summary>
		/// Float 的类型全称
		/// </summary>
		public static readonly string Float = typeof(float).FullName;
		/// <summary>
		/// DateTime 的类型全称
		/// </summary>
		public static readonly string DateTime = typeof(DateTime).FullName;
		/// <summary>
		/// Boolean 的类型全称
		/// </summary>
		public static readonly string Boolean = typeof(bool).FullName;
		/// <summary>
		/// Decimal 的类型全称
		/// </summary>
		public static readonly string Decimal = typeof(decimal).FullName;
		/// <summary>
		/// Cassandra TimeUuid的类型全称
		/// </summary>
		public static readonly string TimeUuid = "Cassandra.TimeUuid";
	}
}
