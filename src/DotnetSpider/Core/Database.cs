using System;
using System.ComponentModel;

namespace DotnetSpider.Core
{
    /// <summary>
    /// Database type enum
    /// </summary>
    /// <summary xml:lang="zh-CN">
    /// 数据库类型
    /// </summary>
    [Flags]
    public enum Database
    {
        /// <summary>
        /// MySql
        /// </summary>
        [Description("MySql.Data.MySqlClient")]
        MySql,

        /// <summary>
        /// SqlServer
        /// </summary>
        [Description("System.Data.SqlClient")]
        SqlServer,

        /// <summary>
        /// MongoDB
        /// </summary>
        Mongo,

        /// <summary>
        /// Cassandra
        /// </summary>
        Cassandra,

        /// <summary>
        /// PostgreSql
        /// </summary>
        PostgreSql,

        /// <summary>
        /// ClickHouse
        /// </summary>
        ClickHouse
    }
}