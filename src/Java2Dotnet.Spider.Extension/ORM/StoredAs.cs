using System;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.ORM
{
	public enum DataType
	{
		String,
		Text,
		Date,
		Time,
		Bool
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class StoredAs : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; }

		public DataType Type { get; set; }

		public uint Lenth { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="length"></param>
		public StoredAs(string name, DataType type, uint length = 0)
		{
			Name = name;
			Type = type;
			Lenth = length;

			if (type != DataType.String && length != 0)
			{
				throw new SpiderExceptoin("Only String can set length.");
			}

			if (type == DataType.String && length == 0)
			{
				throw new SpiderExceptoin("Length can not be 0.");
			}
		}
	}
}
