using DotnetSpider.Core;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Common
{
	public class DataSourceUtil
	{
		public static DbConnection GetConnection(DataSource source, string connectString)
		{
			switch (source)
			{
				case DataSource.MySql:
					{
						return new MySqlConnection(connectString);
					}
				case DataSource.MsSql:
					{
						return new SqlConnection(connectString);
					}
			}

			throw new SpiderException($"Unsported datasource: {source}");
		}
	}
}
