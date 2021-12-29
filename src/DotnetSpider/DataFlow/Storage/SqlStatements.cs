namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// SQL 语句
	/// </summary>
    public class SqlStatements
    {
	    /// <summary>
	    /// 数据库名称 SQL
	    /// </summary>
	    public string DatabaseSql { get; set; }
	    
	    /// <summary>
	    /// 创建表的 SQL 语句
	    /// </summary>
        public string CreateTableSql { get; set; }
        
	    /// <summary>
	    /// 创建数据库的 SQL 语句
	    /// </summary>
        public string CreateDatabaseSql { get; set; }
        
        /// <summary>
        /// 插入的 SQL 语句
        /// </summary>
        public string InsertSql { get; set; }

        /// <summary>
        /// 插入并且忽略重复数据的 SQL 语句
        /// </summary>
        public string InsertIgnoreDuplicateSql { get; set; }

        /// <summary>
        /// 更新的 SQL 语句
        /// </summary>
        public string UpdateSql { get; set; }

        /// <summary>
        /// 插入新的或者更新旧的数据 SQL 语句
        /// </summary>
        public string InsertAndUpdateSql { get; set; }
    }
}