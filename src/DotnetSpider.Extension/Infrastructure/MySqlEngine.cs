using DotnetSpider.Core.Infrastructure;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Linq;

namespace DotnetSpider.Extension.Infrastructure
{
	public class MySqlEngine
	{
		public string Engine { get; set; }
		public string Support { get; set; }

		public static bool IsSupportToku()
		{
			using (var conn = new MySqlConnection(Config.ConnectString))
			{
				return IsSupportToku(conn);
			}
		}

		public static bool IsSupportToku(MySqlConnection conn)
		{
			return conn.Query<MySqlEngine>("show engines").Any(e => e.Engine == "TokuDB" && e.Support != "NO");
		}
	}
}
