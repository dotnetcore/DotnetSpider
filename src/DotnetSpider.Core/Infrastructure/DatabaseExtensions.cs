using DotnetSpider.Downloader;
using Serilog;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using Dapper;
using System.Collections.Generic;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// Database Extentions
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 数据库扩展
	/// </summary>
	public static class DatabaseExtensions
	{
		/// <summary>
		/// Create DbConnection from <see cref="ConnectionStringSettings"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过配置创建连接对象
		/// </summary>
		/// <param name="connectionStringSettings">数据库配置对象 <see cref="ConnectionStringSettings"/></param>
		/// <returns>连接对象 <see cref="DbConnection"/></returns>
		public static DbConnection CreateDbConnection(this ConnectionStringSettings connectionStringSettings)
		{
			if (connectionStringSettings == null)
			{
				throw new SpiderException("ConnectionStringSetting is null");
			}
			if (string.IsNullOrWhiteSpace(connectionStringSettings.ConnectionString) || string.IsNullOrWhiteSpace(connectionStringSettings.ProviderName))
			{
				throw new SpiderException("ConnectionStringSetting is incorrect");
			}

			var factory = DatabaseProviderFactories.GetFactory(connectionStringSettings.ProviderName);

			for (int i = 0; i < 5; ++i)
			{
				try
				{
					DbConnection connection = factory.CreateConnection();
					if (connection != null)
					{
						connection.ConnectionString = connectionStringSettings.ConnectionString;
						if (connection.State == ConnectionState.Closed)
						{
							connection.Open();
						}
						return connection;
					}
				}
				catch (Exception e)
				{
					if (e.Message.ToLower().StartsWith("authentication to host"))
					{
						Log.Logger.Error($"{connectionStringSettings.ConnectionString}: {e}.");
						break;
					}
					if (e.Message.ToLower().StartsWith("access denied for user"))
					{
						Log.Logger.Error($"Access denied: {connectionStringSettings.ConnectionString}.");
						break;
					}
					else
					{
						Log.Logger.Warning($"{connectionStringSettings.ConnectionString}: {e}");
						Thread.Sleep(1000);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Create <see cref="DbConnection"/> instance.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 创建连接对象
		/// </summary>
		/// <param name="source"><see cref="Database"/></param>
		/// <param name="connectString"></param>
		/// <returns></returns>
		public static DbConnection CreateDbConnection(Database source, string connectString)
		{
			DbProviderFactory factory;
			switch (source)
			{
				case Database.MySql:
					{
						factory = DatabaseProviderFactories.GetFactory(DatabaseProviderFactories.MySqlProvider);
						break;
					}
				case Database.SqlServer:
					{
						factory = DatabaseProviderFactories.GetFactory(DatabaseProviderFactories.SqlServerProvider);
						break;
					}
				case Database.PostgreSql:
					{
						factory = DatabaseProviderFactories.GetFactory(DatabaseProviderFactories.PostgreSqlProvider);
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
						Log.Logger.Error($"{e}");
						Thread.Sleep(1000);
					}
					else
					{
						throw;
					}
				}
			}

			throw new SpiderException("Create connection failed");
		}

		/// <summary>
		/// Create <see cref="ConnectionStringSettings"/> instance.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 创建数据库配置对象
		/// </summary>
		/// <param name="source">数据库 <see cref="Database"/></param>
		/// <param name="connectString">连接字符串</param>
		/// <returns>数据库配置对象 <see cref="ConnectionStringSettings"/></returns>
		public static ConnectionStringSettings GetConnectionStringSettings(Database source, string connectString)
		{
			switch (source)
			{
				case Database.MySql:
					{
						return new ConnectionStringSettings("MySql", connectString, DatabaseProviderFactories.MySqlProvider);
					}
				case Database.SqlServer:
					{
						return new ConnectionStringSettings("SqlServer", connectString, DatabaseProviderFactories.SqlServerProvider);
					}
				case Database.PostgreSql:
					{
						return new ConnectionStringSettings("PostgreSql", connectString, DatabaseProviderFactories.PostgreSqlProvider);
					}
				default:
					{
						throw new SpiderException($"Unsported databse: {source}");
					}
			}
		}

		/// <summary>
		/// Build HTML table from sql query result.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 把SQL查询结果拼装成HTML的table
		/// </summary>
		/// <param name="conn">连接对象 <see cref="IDbConnection"/></param>
		/// <param name="sql">SQL语句 </param>
		/// <returns>HTML的table, HTML table</returns>
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
		/// <summary>
		/// 设置DbExecutor是否使用互联网, 如果不使用互联网上传数据则不需要通过NetworkCenter, 提高效率和稳定性
		/// 但这是全局设置, 默认的MySqlEntityPipeline等都是使用此扩展实现的
		/// </summary>
		public static bool UseNetworkCenter = true;

		/// <summary>
		/// Execute parameterized SQL that selects a single value
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns>The first cell selected</returns>
		public static object MyExecuteScalar(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.ExecuteScalar(sql, param, transaction, commandTimeout, commandType));
			}
			else
			{
				return conn.ExecuteScalar(sql, param, transaction, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Execute parameterized SQL and return an System.Data.IDataReader
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns>An System.Data.IDataReader that can be used to iterate over the results of the SQL query.</returns>
		public static IDataReader MyExecuteReader(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.ExecuteReader(sql, param, transaction, commandTimeout, commandType));
			}
			else
			{
				return conn.ExecuteReader(sql, param, transaction, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Execute parameterized SQL
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns>Number of rows affected</returns>
		public static int MyExecute(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.Execute(sql, param, transaction, commandTimeout, commandType));
			}
			else
			{
				return conn.Execute(sql, param, transaction, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Return a dynamic object with properties matching the columns
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns></returns>
		public static dynamic MyQueryFirstOrDefault(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.QueryFirstOrDefault(sql, param, transaction, commandTimeout, commandType));
			}
			else
			{
				return conn.QueryFirstOrDefault(sql, param, transaction, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Executes a query, returning the data typed as per T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="buffered"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).</returns>
		public static IEnumerable<T> MyQuery<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType));
			}
			else
			{
				return conn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Return a sequence of dynamic objects with properties matching the columns
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="buffered"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns></returns>
		public static IEnumerable<dynamic> MyQuery(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.Query(sql, param, transaction, buffered, commandTimeout, commandType));
			}
			else
			{
				return conn.Query(sql, param, transaction, buffered, commandTimeout, commandType);
			}
		}

		/// <summary>
		/// Execute parameterized SQL that selects a single value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="conn"></param>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="transaction"></param>
		/// <param name="commandTimeout"></param>
		/// <param name="commandType"></param>
		/// <returns>The first cell selected</returns>
		public static T MyExecuteScalar<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			if (UseNetworkCenter)
			{
				return NetworkCenter.Current.Execute("db", () => conn.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType));
			}
			else
			{
				return conn.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
			}
		}
	}
}
