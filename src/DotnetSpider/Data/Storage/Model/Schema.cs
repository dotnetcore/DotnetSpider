using System;

namespace DotnetSpider.Data.Storage.Model
{
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

        public Schema(string database, string table)
        {
            Database = database;
            Table = table;
        }
    }
}