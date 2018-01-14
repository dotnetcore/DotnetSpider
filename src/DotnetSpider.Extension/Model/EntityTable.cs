using DotnetSpider.Core.Infrastructure;
using System;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体类对应的表信息
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class EntityTable : System.Attribute
	{
		private string _name;

		/// <summary>
		/// 表名的后缀为空
		/// </summary>
		public const string Empty = "";
		
		/// <summary>
		/// 表名的后缀为星期一的时间
		/// </summary>
		public const string Monday = "Monday";
		
		/// <summary>
		/// 表名的后缀为今天的时间 {name}_20171212
		/// </summary>
		public const string Today = "Today";
		
		/// <summary>
		/// 表名的后缀为当月的第一天 {name}_20171201
		/// </summary>
		public const string FirstDayOfTheMonth = "FirstDayOfTheMonth";
		
		/// <summary>
		/// 表名的后缀为当月 {name}_201712
		/// </summary>
		public const string TheMonth = "TheMonth";

		/// <summary>
		/// 表名的后缀为上个月 {name}_201711
		/// </summary>
		public const string LastMonth = "PreviousMonth";

		/// <summary>
		/// 数据库名
		/// </summary>
		public string Database { get; set; }

		/// <summary>
		/// 表名
		/// </summary>
		public string Name
		{
			get { return _name; }
			set
			{
				if (!string.IsNullOrWhiteSpace(value) && _name != value)
				{
					_name = value.ToLower();
				}
			}
		}

		/// <summary>
		/// 表名后缀
		/// </summary>
		public string Postfix { get; set; }

		/// <summary>
		/// 需要更新的列名 string[]{ "column1", "column2" }
		/// </summary>
		public string[] UpdateColumns { get; set; }

		/// <summary>
		/// 需要创建的索引 string[]{ "column1,column2", "column3" }
		/// </summary>
		public string[] Indexs { get; set; }

		/// <summary>
		/// 需要创建的索引 string[]{ "column1,column2", "column3" }
		/// </summary>
		public string[] Uniques { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="database">数据库名</param>
		/// <param name="name">表名</param>
		/// <param name="postfix">表名后缀</param>
		public EntityTable(string database, string name, string postfix = null)
		{
			Database = database;
			Postfix = postfix;
			Name = name;
		}

		/// <summary>
		/// 计算最终的表名
		/// </summary>
		/// <returns>表名</returns>
		public string CalculateTableName()
		{
			switch (Postfix)
			{
				case FirstDayOfTheMonth:
					{
						return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM_dd}";
					}
				case TheMonth:
					{
						return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM}";

					}
				case LastMonth:
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
