using NLog;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure.Database
{
	/// <summary>
	/// 数据库扩展
	/// </summary>
	public static class DatabaseExtensions
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		/// <summary>
		/// 通过配置创建连接对象
		/// </summary>
		/// <param name="connectionStringSettings">数据库配置对象</param>
		/// <returns>连接对象</returns>
		public static DbConnection CreateDbConnection(this ConnectionStringSettings connectionStringSettings)
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
						Logger.AllLog($"{connectionStringSettings.ConnectionString}: {e}", LogLevel.Error);
						Thread.Sleep(1000);
					}
					else
					{
						Logger.AllLog($"{connectionStringSettings.ConnectionString}: {e}", LogLevel.Warn);
					}
				}
			}

			throw new SpiderException($"Create or open DbConnection failed: {connectionStringSettings.ConnectionString}.");
		}

		/// <summary>
		/// 创建连接对象
		/// </summary>
		/// <param name="source"></param>
		/// <param name="connectString"></param>
		/// <returns></returns>
		public static DbConnection CreateDbConnection(Database source, string connectString)
		{
			DbProviderFactory factory;
			switch (source)
			{
				case Database.MySql:
					{
						factory = DbProviderFactories.GetFactory(DbProviderFactories.MySqlProvider);
						break;
					}
				case Database.SqlServer:
					{
						factory = DbProviderFactories.GetFactory(DbProviderFactories.SqlServerProvider);
						break;
					}
				case Database.PostgreSql:
					{
						factory = DbProviderFactories.GetFactory(DbProviderFactories.PostgreSqlProvider);
						break;
					}
				default:
					{
						throw new SpiderException($"Unsported database: {source}");
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
						Logger.AllLog($"{e}", LogLevel.Error);
						Thread.Sleep(1000);
					}
					else
					{
						throw;
					}
				}
			}

			throw new SpiderException("Create connection failed.");
		}

		/// <summary>
		/// 创建数据库配置对象
		/// </summary>
		/// <param name="source">数据库</param>
		/// <param name="connectString">连接字符串</param>
		/// <returns>数据库配置对象</returns>
		public static ConnectionStringSettings GetConnectionStringSettings(Database source, string connectString)
		{
			switch (source)
			{
				case Database.MySql:
					{
						return new ConnectionStringSettings("MySql", connectString, DbProviderFactories.MySqlProvider);
					}
				case Database.SqlServer:
					{
						return new ConnectionStringSettings("SqlServer", connectString, DbProviderFactories.SqlServerProvider);
					}
				case Database.PostgreSql:
					{
						return new ConnectionStringSettings("PostgreSql", connectString, DbProviderFactories.PostgreSqlProvider);
					}
				default:
					{
						throw new SpiderException($"Unsported databse: {source}");
					}
			}
		}

		/// <summary>
		/// 把SQL查询结果拼装成HTML的table
		/// </summary>
		/// <param name="conn">连接对象</param>
		/// <param name="sql">SQL语句</param>
		/// <returns>HTML的table</returns>
		public static string ToHtml(this IDbConnection conn, string sql)
		{
			var command = conn.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.Text;

			if (conn.State == ConnectionState.Closed)
			{
				conn.Open();
			}
			IDataReader reader = null;
			try
			{
				reader = command.ExecuteReader();

				int row = 1;
				StringBuilder html = new StringBuilder("<table>");
				while (reader.Read())
				{
					if (row == 1)
					{
						html.Append("<tr>");
						for (int i = 1; i < reader.FieldCount + 1; ++i)
						{
							html.Append($"<td>{reader.GetName(i - 1)}</td>");
						}
						html.Append("</tr>");
					}

					html.Append("<tr>");
					for (int j = 1; j < reader.FieldCount + 1; ++j)
					{
						html.Append($"<td>{reader.GetValue(j - 1)}</td>");
					}
					html.Append("</tr>");
					row++;
				}
				html.Append("</table>");

				return html.ToString();
			}
			finally
			{
				reader?.Close();
			}
		}

	}
}
