using Newtonsoft.Json;
using System;

namespace DotnetSpider.Extraction.Model.Attribute
{
	/// <summary>
	/// 爬虫实体类对应的表信息
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TableInfo : System.Attribute
	{
		private string _name;

		[JsonIgnore]
		public override object TypeId => base.TypeId;

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
							var now = DateTime.Now;
							return $"{Name}_{now.AddDays(now.Day * -1 + 1):yyyy_MM_dd}";
						}
					case TableNamePostfix.Month:
						{
							var now = DateTime.Now;
							return $"{Name}_{now.AddDays(now.Day * -1 + 1):yyyy_MM}";
						}
					case TableNamePostfix.LastMonth:
						{
							var now = DateTime.Now;
							return $"{Name}_{now.AddDays(now.Day * -1 + 1).AddMonths(-1):yyyy_MM}";
						}
					case TableNamePostfix.Monday:
						{
							var now = DateTime.Now;
							int i = now.DayOfWeek - DayOfWeek.Monday == -1 ? 6 : -1;
							TimeSpan ts = new TimeSpan(i, 0, 0, 0);
							return $"{Name}_{DateTime.Now.Subtract(ts).Date:yyyy_MM_dd}";
						}
					case TableNamePostfix.Today:
						{
							var now = DateTime.Now;
							return $"{Name}_{now:yyyy_MM_dd}";
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
