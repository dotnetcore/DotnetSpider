using System;

namespace Java2Dotnet.Spider.Extension.ORM
{
	[AttributeUsage(AttributeTargets.Property)]
	public class StoredAs : Attribute
	{
		public enum ValueType { Text, String, Time, Date, Float, Double, Bool, Int, BigInt }

		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; }

		public ValueType Type { get; set; }

		public uint Lenth { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="length"></param>
		public StoredAs(string name, ValueType type, uint length = 11)
		{
			Name = name;
			Type = type;
			Lenth = length;
		}
	}
}
