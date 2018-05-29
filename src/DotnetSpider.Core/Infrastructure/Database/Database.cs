using System;
using System.ComponentModel;

namespace DotnetSpider.Core.Infrastructure.Database
{
	[Flags]
	public enum Database
	{
		[Description("MySql.Data.MySqlClient")]
		MySql,
		[Description("System.Data.SqlClient")]
		SqlServer,
		MongoDb,
        Cassandra,
        PostgreSql

    }
}
