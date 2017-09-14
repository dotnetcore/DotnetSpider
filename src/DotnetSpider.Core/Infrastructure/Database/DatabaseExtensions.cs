using NLog;
using System;
using System.Configuration;
using System.Data.Common;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure.Database
{
	public static class DatabaseExtensions
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		public static DbConnection GetDbConnection(this ConnectionStringSettings connectionStringSettings)
		{
			if (connectionStringSettings == null)
			{
				throw new SpiderException("ConnectionStringSetting is null.");
			}
			if (string.IsNullOrEmpty(connectionStringSettings.ConnectionString) || string.IsNullOrEmpty(connectionStringSettings.ProviderName))
			{
				throw new SpiderException("ConnectionStringSetting is incorrect.");
			}

			var factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

			for (int i = 0; i < 5; ++i)
			{
				try
				{
					DbConnection connection = factory.CreateConnection();
					if (connection != null)
					{
						connection.ConnectionString = connectionStringSettings.ConnectionString;
						connection.Open();
						return connection;
					}
				}
				catch (Exception e)
				{
					if (e.Message.ToLower().StartsWith("authentication to host"))
					{
						Logger.MyLog($"{e}", LogLevel.Error);
						Thread.Sleep(1000);
					}
					else
					{
						//throw;
					}
				}
			}

			throw new SpiderException("Can't get db connection.");
		}

		public static DbConnection GetDbConnection(Database source, string connectString)
		{
			DbProviderFactory factory;
			switch (source)
			{
				case Database.MySql:
					{
						factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
						break;
					}
				case Database.SqlServer:
					{
						factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
						break;
					}
				default:
					{
						throw new SpiderException($"Unsported databse: {source}");
					}
			}

			for (int i = 0; i < 5; ++i)
			{
				try
				{
					var connection = factory.CreateConnection();
					if (connection != null)
					{
						connection.ConnectionString = connectString;
						connection.Open();
						return connection;
					}
				}
				catch (Exception e)
				{
					if (e.Message.ToLower().StartsWith("authentication to host"))
					{
						Logger.MyLog($"{e}", LogLevel.Error);
						Thread.Sleep(1000);
					}
					else
					{
						throw;
					}
				}
			}

			throw new SpiderException("Can't get db connection.");
		}

		public static ConnectionStringSettings GetConnectionStringSettings(Database source, string connectString)
		{
			switch (source)
			{
				case Database.MySql:
					{
						return new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
					}
				case Database.SqlServer:
					{
						return new ConnectionStringSettings("SqlServer", connectString, "System.Data.SqlClient");
					}
				default:
					{
						throw new SpiderException($"Unsported databse: {source}");
					}
			}
		}
	}
}
