using System;

namespace DotnetSpider.Common
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
		[System.ComponentModel.Description("MySql.Data.MySqlClient")]
		MySql,

		/// <summary>
		/// SqlServer
		/// </summary>
		[System.ComponentModel.Description("System.Data.SqlClient")]
		SqlServer,

		/// <summary>
		/// MongoDB
		/// </summary>
		MongoDb,

		/// <summary>
		/// Cassandra
		/// </summary>
		Cassandra,

		/// <summary>
		/// PostgreSql
		/// </summary>
		PostgreSql,

		/// <summary>
		/// Clickhouse
		/// </summary>
		Clickhouse,
	}
}
