using DotnetSpider.Extension.Model;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 数据库管道使用的实体中间信息
	/// </summary>
	public class EntityAdapter
	{
		/// <summary>
		/// 数据库表的信息
		/// </summary>
		public EntityTable Table { get; }

		/// <summary>
		/// 爬虫实体类定义的列信息
		/// </summary>
		public List<Column> Columns { get; }

		/// <summary>
		/// 插入的SQL语句
		/// </summary>
		internal string InsertSql { get; set; }

		/// <summary>
		/// 插入并且忽略重复数据的SQL
		/// </summary>
		internal string InsertAndIgnoreDuplicateSql { get; set; }

		/// <summary>
		/// 更新的SQL
		/// </summary>
		internal string UpdateSql { get; set; }

		/// <summary>
		/// 查询的SQL
		/// </summary>
		internal string SelectSql { get; set; }

		/// <summary>
		/// 插入新的或者更新旧的数据SQL
		/// </summary>
		internal string InsertNewAndUpdateOldSql { get; set; }

		/// <summary>
		/// 数据管理模式
		/// </summary>
		public PipelineMode PipelineMode { get; set; } = PipelineMode.InsertAndIgnoreDuplicate;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="table">数据库表的信息</param>
		/// <param name="columns">爬虫实体类定义的列信息</param>
		public EntityAdapter(EntityTable table, List<Column> columns)
		{
			Table = table;
			Columns = columns;
		}
	}
}
