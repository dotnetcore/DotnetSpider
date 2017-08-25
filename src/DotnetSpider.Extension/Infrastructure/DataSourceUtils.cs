using DotnetSpider.Core;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlClient;

namespace DotnetSpider.Extension.Infrastructure
{
	public class DataSourceUtils
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
