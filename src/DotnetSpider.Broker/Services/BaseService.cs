using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public abstract class BaseService
	{
		protected readonly BrokerOptions _options;

		protected BaseService(BrokerOptions options)
		{
			_options = options;
		}

		protected IDbConnection CreateDbConnection()
		{
			switch (_options.StorageType)
			{
				case StorageType.MySql:
					{
						return new MySqlConnection(_options.ConnectionString);
					}
				case StorageType.SqlServer:
					{
						return new SqlConnection(_options.ConnectionString);
					}
			}
			throw new NotSupportedException($"notsupported storage {_options.StorageType}.");
		}
	}
}
