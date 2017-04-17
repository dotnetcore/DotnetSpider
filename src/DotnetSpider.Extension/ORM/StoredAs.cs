using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.ORM
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

		public uint Length { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="length"></param>
		public StoredAs(string name, DataType type, uint length = 255)
		{
			Name = name;
			Type = type;
			Length = length;

			if (type == DataType.String && length == 0)
			{
				throw new SpiderException("Length can not be 0.");
			}
		}

		public override string ToString()
		{
			string reslut = "";

			switch (Type)
			{
				case DataType.Bool:
					{
						reslut = "BOOL";
						break;
					}
				case DataType.Date:
					{
						reslut = "DATE";
						break;
					}
				case DataType.Time:
					{
						reslut = "TIME";
						break;
					}
				case DataType.Text:
					{
						reslut = "TEXT";
						break;
					}

				case DataType.String:
					{
						reslut = $"STRING,{Length}";
						break;
					}
			}

			return reslut;
		}
	}
}
