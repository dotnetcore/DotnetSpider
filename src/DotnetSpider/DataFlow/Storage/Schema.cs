using System;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// Schema 信息
	/// </summary>
	public class Schema : Attribute
	{
		/// <summary>
		/// 数据库名
		/// </summary>
		public string Database { get; }

		/// <summary>
		/// 表名
		/// </summary>
		public string Table { get; }

		/// <summary>
		/// 表名后缀
		/// </summary>
		public TablePostfix TablePostfix { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="database">数据库名</param>
		/// <param name="table">表名</param>
		/// <param name="tablePostfix">表名后缀</param>
		public Schema(string database, string table, TablePostfix tablePostfix = TablePostfix.None)
		{
			Database = database;
			Table = table;
			TablePostfix = tablePostfix;
		}
	}
}