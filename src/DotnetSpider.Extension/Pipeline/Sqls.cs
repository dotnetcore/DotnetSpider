namespace DotnetSpider.Extension.Pipeline
{
	public class Sqls
	{
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
		internal string SelectSql;

		/// <summary>
		/// 插入新的或者更新旧的数据SQL
		/// </summary>
		internal string InsertNewAndUpdateOldSql { get; set; }
	}
}
