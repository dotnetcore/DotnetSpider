namespace DotnetSpider.Extension.Pipeline
{
	public class SqlStatements
	{
		/// <summary>
		/// 插入的SQL语句
		/// </summary>
		public string InsertSql { get; set; }

		/// <summary>
		/// 插入并且忽略重复数据的SQL
		/// </summary>
		public string InsertAndIgnoreDuplicateSql { get; set; }

		/// <summary>
		/// 更新的SQL
		/// </summary>
		public string UpdateSql { get; set; }

		/// <summary>
		/// 查询的SQL
		/// </summary>
		public string SelectSql;

		/// <summary>
		/// 插入新的或者更新旧的数据SQL
		/// </summary>
		public string InsertNewAndUpdateOldSql { get; set; }
	}
}
