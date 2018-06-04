using DotnetSpider.Core.Infrastructure;
using System;

namespace DotnetSpider.Extension.Model
{
	public enum TableNamePostfix
	{
		None,

		/// <summary>
		/// 表名的后缀为星期一的时间
		/// </summary>
		Monday,

		/// <summary>
		/// 表名的后缀为今天的时间 {name}_20171212
		/// </summary>
		Today,

		/// <summary>
		/// 表名的后缀为当月的第一天 {name}_20171201
		/// </summary>
		FirstDayOfTheMonth,

		/// <summary>
		/// 表名的后缀为当月 {name}_201712
		/// </summary>
		Month,

		/// <summary>
		/// 表名的后缀为上个月 {name}_201711
		/// </summary>
		LastMonth
	}

	/// <summary>
	/// 爬虫实体类对应的表信息
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TableInfo : System.Attribute
	{
		private string _name;

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
		public TableNamePostfix Postfix { get; set; }

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
		public TableInfo(string database, string name, TableNamePostfix postfix = TableNamePostfix.None)
		{
			Database = database;
			Postfix = postfix;
			Name = name;
		}

		/// <summary>
		/// 计算最终的表名
		/// </summary>
		/// <returns>表名</returns>
		public string FullName
		{
			get
			{
				switch (Postfix)
				{
					case TableNamePostfix.FirstDayOfTheMonth:
						{
							return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM_dd}";
						}
					case TableNamePostfix.Month:
						{
							return $"{Name}_{DateTimeUtil.FirstDayOfTheMonth:yyyy_MM}";

						}
					case TableNamePostfix.LastMonth:
						{
							return $"{Name}_{DateTimeUtil.FirstDayOfLastMonth:yyyy_MM}";
						}
					case TableNamePostfix.Monday:
						{
							return $"{Name}_{DateTimeUtil.Monday:yyyy_MM_dd}";
						}
					case TableNamePostfix.Today:
						{
							return $"{Name}_{DateTimeUtil.Today:yyyy_MM_dd}";
						}
					default:
						{
							return Name;
						}
				}
			}
		}
	}
}
