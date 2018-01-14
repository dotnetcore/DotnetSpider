using Dapper;
using System.Data;
using DotnetSpider.Core.Redial;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.Extension
{
	/// <summary>
	/// 数据库执行扩展
	/// </summary>
	public static class DbExecutor
	{
		public static bool UseNetworkCenter = true;

		internal static object MyExecuteScalar(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static IDataReader MyExecuteReader(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static int MyExecute(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static dynamic MyQueryFirstOrDefault(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static IEnumerable<T> MyQuery<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static IEnumerable<dynamic> MyQuery(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
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

		internal static T MyExecuteScalar<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
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
