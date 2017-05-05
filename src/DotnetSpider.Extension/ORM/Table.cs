using System;

namespace DotnetSpider.Extension.ORM
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Table : Attribute
	{
		public string Database { get; set; }

		public string Name { get; set; }

		public TableSuffix Suffix { get; set; }

		public string Primary { get; set; }

		public string[] UpdateColumns { get; set; }

		public string[] Indexs { get; set; }

		public string[] Uniques { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="database"></param>
		/// <param name="name"></param>
		/// <param name="suffix"></param>
		public Table(string database, string name, TableSuffix suffix = TableSuffix.Empty)
		{
			Database = database;
			Suffix = suffix;
			Name = name;
		}
	}
}
