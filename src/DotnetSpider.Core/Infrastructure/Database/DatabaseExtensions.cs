using Serilog;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace DotnetSpider.Core.Infrastructure.Database
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

			var factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

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
		public static DbConnection CreateDbConnection(Common.Database source, string connectString)
		{
			DbProviderFactory factory;
			switch (source)
			{
				case Common.Database.MySql:
					{
						factory = DbProviderFactories.GetFactory(DbProviderFactories.MySqlProvider);
						break;
					}
				case Common.Database.SqlServer:
					{
						factory = DbProviderFactories.GetFactory(DbProviderFactories.SqlServerProvider);
						break;
					}
				case Common.Database.PostgreSql:
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
		public static ConnectionStringSettings GetConnectionStringSettings(Common.Database source, string connectString)
		{
			switch (source)
			{
				case Common.Database.MySql:
					{
						return new ConnectionStringSettings("MySql", connectString, DbProviderFactories.MySqlProvider);
					}
				case Common.Database.SqlServer:
					{
						return new ConnectionStringSettings("SqlServer", connectString, DbProviderFactories.SqlServerProvider);
					}
				case Common.Database.PostgreSql:
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

	}
}
