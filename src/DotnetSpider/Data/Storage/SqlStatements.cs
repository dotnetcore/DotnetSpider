namespace DotnetSpider.Data.Storage
{
    public class SqlStatements
    {
        public string CreateTableSql { get; set; }
        
        public string CreateDatabaseSql { get; set; }
        
        /// <summary>
        /// 插入的SQL语句
        /// </summary>
        public string InsertSql { get; set; }

        /// <summary>
        /// 插入并且忽略重复数据的SQL
        /// </summary>
        public string InsertIgnoreDuplicateSql { get; set; }

        /// <summary>
        /// 更新的SQL
        /// </summary>
        public string UpdateSql { get; set; }

        /// <summary>
        /// 插入新的或者更新旧的数据SQL
        /// </summary>
        public string InsertAndUpdateSql { get; set; }
    }
}