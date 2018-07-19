using System.Data;
using System.Collections.Generic;
using DotnetSpider.Downloader;
using Dapper;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// 数据库执行扩展
	/// </summary>
	public static class DbConnectionExtensions
	{
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
