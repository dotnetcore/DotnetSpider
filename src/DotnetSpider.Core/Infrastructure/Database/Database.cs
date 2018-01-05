using System;
using System.ComponentModel;

namespace DotnetSpider.Core.Infrastructure.Database
{
	/// <summary>
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
		PostgreSql
	}
}
