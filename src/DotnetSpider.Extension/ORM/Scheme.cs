using System;

namespace DotnetSpider.Extension.ORM
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Schema : Attribute
	{
		public string Database { get; set; }

		public string TableName { get; set; }

		public TableSuffix Suffix { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="database"></param>
		/// <param name="tableName"></param>
		/// <param name="suffix"></param>
		public Schema(string database, string tableName, TableSuffix suffix = TableSuffix.Empty)
		{
			Database = database;
			Suffix = suffix;
			TableName = tableName;
		}
	}
}
