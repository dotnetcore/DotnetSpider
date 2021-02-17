using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetSpider.PostgreSql
{
	/// <summary>
	/// PostgreSql 保存解析(实体)结果
	/// </summary>
	public class PostgreSqlEntityStorage : RelationalDatabaseEntityStorageBase
	{
		/// <summary>
		/// 根据配置返回存储器¬
		/// </summary>
		/// <param name="configuration">配置</param>
		/// <returns></returns>
		public static IDataFlow CreateFromOptions(IConfiguration configuration)
		{
			var options = new PostgreOptions(configuration);
			return new PostgreSqlEntityStorage(options.Mode, options.ConnectionString)
			{
				UseTransaction = options.UseTransaction,
				IgnoreCase = options.IgnoreCase,
				RetryTimes = options.RetryTimes
			};
		}

		public override Task InitializeAsync()
		{
			return Task.CompletedTask;
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

		protected virtual string GenerateCreateTableSql(TableMetadata tableMetadata)
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

		protected virtual string Escape => "\"";

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="mode">存储器类型</param>
		/// <param name="connectionString">连接字符串</param>
		public PostgreSqlEntityStorage(StorageMode mode,
			string connectionString) : base(mode,
			connectionString)
		{
		}

		protected virtual string GenerateCreateDatabaseSql(TableMetadata tableMetadata)
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
		protected virtual string GenerateDataTypeSql(string type, int length)
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

		protected override SqlStatements GenerateSqlStatements(TableMetadata tableMetadata)
		{
			var sqlStatements = new SqlStatements
			{
				InsertSql = GenerateInsertSql(tableMetadata, false),
				InsertIgnoreDuplicateSql = GenerateInsertSql(tableMetadata, true),
				InsertAndUpdateSql = GenerateInsertAndUpdateSql(tableMetadata),
				UpdateSql = GenerateUpdateSql(tableMetadata),
				CreateTableSql = GenerateCreateTableSql(tableMetadata),
				CreateDatabaseSql = GenerateCreateDatabaseSql(tableMetadata),
				DatabaseSql = string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
					? ""
					: $"{Escape}{GetNameSql(tableMetadata.Schema.Database)}{Escape}"
			};
			return sqlStatements;
		}

		/// <summary>
		/// 生成数据库名称的 SQL
		/// </summary>
		/// <param name="tableMetadata">表元数据</param>
		/// <returns>SQL 语句</returns>
		protected virtual string GenerateTableSql(TableMetadata tableMetadata)
		{
			var tableName = GetNameSql(GetTableName(tableMetadata));
			var database = GetNameSql(tableMetadata.Schema.Database);
			return string.IsNullOrWhiteSpace(database)
				? $"{Escape}{tableName}{Escape}"
				: $"{Escape}{database}{Escape}.{Escape}{tableName}{Escape}";
		}

		/// <summary>
		/// 生成列的 SQL
		/// </summary>
		/// <returns>SQL 语句</returns>
		protected virtual string GenerateColumnSql(Column column, bool isPrimary)
		{
			var columnName = GetNameSql(column.Name);
			var dataType = GenerateDataTypeSql(column.Type, column.Length);
			if (isPrimary || column.Required)
			{
				dataType = $"{dataType} NOT NULL";
			}

			return $"{Escape}{columnName}{Escape} {dataType}";
		}

		/// <summary>
		/// 生成插入数据的 SQL 语句
		/// </summary>
		/// <param name="tableMetadata">表元数据</param>
		/// <param name="ignoreDuplicate">是否忽略重复键的数据</param>
		/// <returns>SQL 语句</returns>
		protected virtual string GenerateInsertSql(TableMetadata tableMetadata, bool ignoreDuplicate)
		{
			var columns = tableMetadata.Columns;
			var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
			// 去掉自增主键
			var insertColumns =
				(isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
				.ToArray();

			var columnsSql = string.Join(", ", insertColumns.Select(c => $"{Escape}{GetNameSql(c.Key)}{Escape}"));

			var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

			var tableSql = GenerateTableSql(tableMetadata);

			var sql =
				$"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql});";
			return sql;
		}

		/// <summary>
		/// 生成更新数据的 SQL 语句
		/// </summary>
		/// <param name="tableMetadata">表元数据</param>
		/// <returns>SQL 语句</returns>
		protected virtual string GenerateUpdateSql(TableMetadata tableMetadata)
		{
			if (tableMetadata.Updates == null || tableMetadata.Updates.Count == 0)
			{
				Logger?.LogWarning("实体没有设置主键, 无法生成 Update 语句");
				return null;
			}

			var where = "";
			foreach (var column in tableMetadata.Primary)
			{
				where += $" {Escape}{GetNameSql(column)}{Escape} = @{column} AND";
			}

			where = where.Substring(0, where.Length - 3);

			var setCols = string.Join(", ",
				tableMetadata.Updates.Select(c => $"{Escape}{GetNameSql(c)}{Escape}= @{c}"));
			var tableSql = GenerateTableSql(tableMetadata);
			var sql = $"UPDATE {tableSql} SET {setCols} WHERE {where};";
			return sql;
		}

		/// <summary>
		/// 生成插入新数据或者更新旧数据的 SQL 语句
		/// </summary>
		/// <param name="tableMetadata">表元数据</param>
		/// <returns>SQL 语句</returns>
		protected virtual string GenerateInsertAndUpdateSql(TableMetadata tableMetadata)
		{
			if (!tableMetadata.HasPrimary)
			{
				Logger?.LogWarning("实体没有设置主键, 无法生成 InsertAndUpdate 语句");
				return null;
			}

			var columns = tableMetadata.Columns;
			var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
			// 去掉自增主键
			var insertColumns =
				(isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
				.ToArray();

			var columnsSql = string.Join(", ", insertColumns.Select(c => $"{Escape}{GetNameSql(c.Key)}{Escape}"));

			var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

			var tableSql = GenerateTableSql(tableMetadata);
			var setCols = string.Join(", ",
				insertColumns.Select(c => $"{Escape}{GetNameSql(c.Key)}{Escape}= @{c.Key}"));
			var sql =
				$"INSERT INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql}) ON DUPLICATE key UPDATE {setCols};";
			return sql;
		}
	}
}
