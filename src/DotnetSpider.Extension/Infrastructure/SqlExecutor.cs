using Dapper;
using System.Data;
using DotnetSpider.Core.Redial;
using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	public static class SqlExecutor
	{
		internal static object MyExecuteScalar(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.ExecuteScalar(conn, sql, param, transaction, commandTimeout, commandType);
			});
		}

		internal static IDataReader MyExecuteReader(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.ExecuteReader(conn, sql, param, transaction, commandTimeout, commandType);
			});
		}

		internal static int MyExecute(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.Execute(conn, sql, param, transaction, commandTimeout, commandType);
			});
		}

		internal static dynamic MyQueryFirstOrDefault(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.QueryFirstOrDefault(conn, sql, param, transaction, commandTimeout, commandType);
			});
		}

		internal static IEnumerable<T> MyQuery<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.Query<T>(conn, sql, param, transaction, buffered, commandTimeout, commandType);
			});
		}

		internal static IEnumerable<dynamic> MyQuery(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.Query(conn, sql, param, transaction, buffered, commandTimeout, commandType);
			});
		}

		internal static T MyExecuteScalar<T>(this IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			return NetworkCenter.Current.Execute("db", () =>
			{
				return SqlMapper.ExecuteScalar<T>(conn, sql, param, transaction, commandTimeout, commandType);
			});
		}
	}
}
