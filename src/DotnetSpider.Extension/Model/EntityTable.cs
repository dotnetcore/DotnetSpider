using DotnetSpider.Core.Infrastructure;
using System;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EntityTable : System.Attribute
	{
		public const string Empty = "";
		public const string Monday = "Monday";
		public const string Today = "Today";
		public const string FirstDayOfCurrentMonth = "FirstDayOfCurrentMonth";
		public const string CurrentMonth = "CurrentMonth";
		public const string PreviousMonth = "PreviousMonth";

		public string Database { get; set; }

		public string Name { get; set; }

		public string Postfix { get; set; }

		public string[] UpdateColumns { get; set; }

		public string[] Indexs { get; set; }

		public string[] Uniques { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="database"></param>
		/// <param name="name"></param>
		/// <param name="postfix"></param>
		public EntityTable(string database, string name, string postfix = null)
		{
			Database = database;
			Postfix = postfix;
			Name = name;
		}

		public string CalculateTableName()
		{
			switch (Postfix)
			{
				case FirstDayOfCurrentMonth:
					{
						return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM_dd}";
					}
				case CurrentMonth:
					{
						return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM}";

					}
				case PreviousMonth:
					{
						return $"{Name}_{DateTimeUtil.FirstDayOfLastMonth:yyyy_MM}";
					}
				case Monday:
					{
						return $"{Name}_{DateTimeUtil.Monday:yyyy_MM_dd}";
					}
				case Today:
					{
						return $"{Name}_{DateTime.Now:yyyy_MM_dd}";
					}
			}
			return $"{Name}{Postfix}";
		}
	}
}
