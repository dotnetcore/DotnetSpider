using DotnetSpider.Extension.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体类对应的表信息
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class Schema : System.Attribute
	{
		/// <summary>
		/// 重载是为了添加 JsonIgnore 标签
		/// </summary>
		[JsonIgnore]
		public override object TypeId => base.TypeId;

		/// <summary>
		/// 数据库名
		/// </summary>
		public string Database { get; set; }

		/// <summary>
		/// 表名
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// 表名后缀
		/// </summary>
		public TableNamePostfix TableNamePostfix { get; set; }

		public Schema()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="database">数据库名</param>
		/// <param name="tableName">表名</param>
		/// <param name="postfix">表名后缀</param>
		public Schema(string database, string tableName, TableNamePostfix postfix = TableNamePostfix.None)
		{
			Database = database;
			TableName = tableName;
			TableNamePostfix = postfix;
		}

		/// <summary>
		/// 计算最终的表名
		/// </summary>
		/// <returns>表名</returns>
		public string FullName
		{
			get
			{
				switch (TableNamePostfix)
				{
					case TableNamePostfix.FirstDayOfTheMonth:
						{
							var now = DateTime.Now;
							return $"{TableName}_{now.AddDays(now.Day * -1 + 1):yyyy_MM_dd}";
						}
					case TableNamePostfix.Month:
						{
							var now = DateTime.Now;
							return $"{TableName}_{now.AddDays(now.Day * -1 + 1):yyyy_MM}";
						}
					case TableNamePostfix.LastMonth:
						{
							var now = DateTime.Now;
							return $"{TableName}_{now.AddDays(now.Day * -1 + 1).AddMonths(-1):yyyy_MM}";
						}
					case TableNamePostfix.Monday:
						{
							var now = DateTime.Now;
							int i = now.DayOfWeek - DayOfWeek.Monday == -1 ? 6 : -1;
							TimeSpan ts = new TimeSpan(i, 0, 0, 0);
							return $"{TableName}_{DateTime.Now.Subtract(ts).Date:yyyy_MM_dd}";
						}
					case TableNamePostfix.Today:
						{
							var now = DateTime.Now;
							return $"{TableName}_{now:yyyy_MM_dd}";
						}
					default:
						{
							return TableName;
						}
				}
			}
		}
	}
}
