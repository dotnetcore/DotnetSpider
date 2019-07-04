using System;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Model;
using Npgsql;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.Postgre
{
	/// <summary>
	/// PostgreSql 保存解析(实体)结果
	/// </summary>
	public class PostgreSqlEntityStorage : MySqlEntityStorage
	{
		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public new static PostgreSqlEntityStorage CreateFromOptions(ISpiderOptions options)
		{
			return new PostgreSqlEntityStorage(options.StorageType, options.StorageConnectionString)
			{
				IgnoreCase = options.StorageIgnoreCase,
				RetryTimes = options.StorageRetryTimes,
				UseTransaction = options.StorageUseTransaction
			};
		}

		/// <summary>
		/// 创建数据库和表
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sqlStatements">SQL 语句</param>
		protected override void EnsureDatabaseAndTableCreated(IDbConnection conn,
			SqlStatements sqlStatements)
		{
			if (!string.IsNullOrWhiteSpace(sqlStatements.CreateDatabaseSql))
			{
				try
				{
					conn.Execute(sqlStatements.CreateDatabaseSql);
					conn.Execute(sqlStatements.CreateDatabaseSql);
				}
				catch (Exception e)
				{
					if (e.Message != $"42P04: database {sqlStatements.DatabaseSql} already exists")
					{
						throw;
					}
				}
			}

			conn.Execute(sqlStatements.CreateTableSql);
		}

		protected override string GenerateCreateTableSql(TableMetadata tableMetadata)
		{
			var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;

			var tableSql = GenerateTableSql(tableMetadata);

			var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableSql} (");

			foreach (var column in tableMetadata.Columns)
			{
				var isPrimary = tableMetadata.IsPrimary(column.Key);

				if (isPrimary)
				{
					var primarySql = $"CONSTRAINT {GetTableName(tableMetadata).ToUpper()}_PK PRIMARY KEY, ";
					builder.Append(isAutoIncrementPrimary
						? $"{GetNameSql(column.Value.Name)} SERIAL {primarySql}"
						: $"{GenerateColumnSql(column.Value, true)} {(tableMetadata.Primary.Count > 1 ? "" : primarySql)}");
				}
				else
				{
					builder.Append($"{GenerateColumnSql(column.Value, false)}, ");
				}
			}

			builder.Remove(builder.Length - 2, 2);

			if (tableMetadata.Primary != null && tableMetadata.Primary.Count > 1)
			{
				builder.Append(
					$", CONSTRAINT {GetTableName(tableMetadata).ToUpper()}_PK PRIMARY KEY ({string.Join(", ", tableMetadata.Primary.Select(c => $"{Escape}{GetNameSql(c)}{Escape}"))})");
			}

			if (tableMetadata.Indexes.Count > 0)
			{
				foreach (var index in tableMetadata.Indexes.Where(x => x.IsUnique))
				{
					var name = index.Name;
					var columnNames = string.Join(", ", index.Columns.Select(c => $"{Escape}{GetNameSql(c)}{Escape}"));
					builder.Append(
						$", CONSTRAINT {Escape}{name}{Escape} UNIQUE ({columnNames})");
				}
			}

			builder.Append(");");
			if (tableMetadata.Indexes.Count > 0)
			{
				foreach (var index in tableMetadata.Indexes.Where(x => x.IsUnique))
				{
					var name = index.Name;
					var columnNames = string.Join(", ", index.Columns.Select(c => $"{Escape}{GetNameSql(c)}{Escape}"));
					builder.Append(
						$"CREATE INDEX {name} ON {tableSql} ({columnNames});");
				}
			}

			var sql = builder.ToString();
			return sql;
		}

		protected override IDbConnection CreateDbConnection(string connectString)
		{
			return new NpgsqlConnection(connectString);
		}

		protected override string Escape => "\"";

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="storageType">存储器类型</param>
		/// <param name="connectionString">连接字符串</param>
		public PostgreSqlEntityStorage(StorageType storageType = StorageType.InsertIgnoreDuplicate,
			string connectionString = null) : base(storageType,
			connectionString)
		{
		}

		protected override string GenerateCreateDatabaseSql(TableMetadata tableMetadata)
		{
			return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
				? ""
				: $"CREATE DATABASE {Escape}{GetNameSql(tableMetadata.Schema.Database)}{Escape} with encoding 'UTF-8';";
		}

		/// <summary>
		/// 生成数据类型的 SQL
		/// </summary>
		/// <param name="type">数据类型</param>
		/// <param name="length">数据长度</param>
		/// <returns>SQL 语句</returns>
		protected override string GenerateDataTypeSql(string type, int length)
		{
			string dataType;

			switch (type)
			{
				case BoolType:
				{
					dataType = "BOOL";
					break;
				}
				case DateTimeType:
				{
					dataType = "TIMESTAMP DEFAULT CURRENT_TIMESTAMP";
					break;
				}
				case DateTimeOffsetType:
				{
					dataType = "TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP";
					break;
				}
				case DecimalType:
				{
					dataType = "NUMERIC";
					break;
				}
				case DoubleType:
				{
					dataType = "FLOAT8";
					break;
				}
				case FloatType:
				{
					dataType = "FLOAT4";
					break;
				}
				case IntType:
				{
					dataType = "INT4";
					break;
				}
				case LongType:
				{
					dataType = "INT8";
					break;
				}
				case ByteType:
				{
					dataType = "INT2";
					break;
				}
				case ShortType:
				{
					dataType = "INT2";
					break;
				}
				default:
				{
					dataType = length <= 0 || length > 8000 ? "TEXT" : $"VARCHAR({length})";
					break;
				}
			}

			return dataType;
		}
	}
}