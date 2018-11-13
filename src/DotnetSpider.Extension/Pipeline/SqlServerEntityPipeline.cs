using System;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到SqlServer中
	/// </summary>
	public class SqlServerEntityPipeline : DbEntityPipeline
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串, 如果为空框架会尝试从配置文件中读取</param>
		/// <param name="pipelineMode">数据管道模式</param>
		public SqlServerEntityPipeline(string connectString = null,
			PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate) : base(connectString, pipelineMode)
		{
		}

		private string GenerateCreateDatabaseSql(TableInfo tableInfo, string serverVersion)
		{
			var database = GetDatabaseName(tableInfo);
			if (string.IsNullOrWhiteSpace(database))
			{
				return "SELECT CURRENT_TIMESTAMP";
			}

			var version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
				{
					return
						$"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{database}') CREATE DATABASE {database};";
				}
				default:
				{
					return
						$"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{database}') CREATE DATABASE {database};";
				}
			}
		}

		private string GenerateIfDatabaseExistsSql(TableInfo tableInfo, string serverVersion)
		{
			var database = GetDatabaseName(tableInfo);
			if (string.IsNullOrWhiteSpace(database))
			{
				return "SELECT COUNT(CURRENT_TIMESTAMP)";
			}

			var version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
				{
					return $"SELECT COUNT(*) FROM sysdatabases WHERE name='{database}'";
				}
				default:
				{
					return $"SELECT COUNT(*) FROM sys.databases WHERE name='{database}'";
				}
			}
		}

		private string GenerateCreateTableSql(TableInfo tableInfo)
		{
			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var isAutoIncrementPrimary = tableInfo.IsAutoIncrementPrimary;

			var builder = string.IsNullOrWhiteSpace(database)
				? new StringBuilder($"IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (")
				: new StringBuilder(
					$"USE {database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");

			foreach (var column in tableInfo.Columns)
			{
				var isPrimary = tableInfo.Primary.Any(k => k.Name == column.Name);

				var columnSql = GenerateColumn(column, isPrimary);

				if (isAutoIncrementPrimary && isPrimary)
				{
					builder.Append($"{columnSql} IDENTITY(1,1), ");
				}
				else
				{
					builder.Append($"{columnSql}, ");
				}
			}

			builder.Remove(builder.Length - 2, 2);

			if (AutoTimestamp)
			{
				builder.Append(", creation_time DATETIME DEFAULT(GETDATE()), creation_date DATE DEFAULT(GETDATE())");
			}

			if (tableInfo.Primary.Count > 0)
			{
				var primaryKeys = string.Join(", ", tableInfo.Primary.Select(c => $"[{GetColumnName(c)}]"));
				builder.Append(
					$", CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({primaryKeys}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON , ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");
			}
			else
			{
				builder.Append(") ON [PRIMARY];");
			}

			if (tableInfo.Indexes.Count > 0)
			{
				foreach (var index in tableInfo.Indexes)
				{
					var name = index.Key;
					var indexColumnNames = string.Join(", ", index.Value.Select(c => $"[{GetColumnName(c)}]"));
					builder.Append($"CREATE NONCLUSTERED INDEX [INDEX_{name}] ON {tableName} ({indexColumnNames});");
				}
			}

			if (tableInfo.Uniques.Count > 0)
			{
				foreach (var unique in tableInfo.Uniques)
				{
					var name = unique.Key;
					var uniqueColumnNames = string.Join(", ", unique.Value.Select(c => $"[{GetColumnName(c)}]"));
					builder.Append(
						$"CREATE UNIQUE NONCLUSTERED INDEX [UNIQUE_{name}] ON {tableName} ({uniqueColumnNames}) {(PipelineMode == PipelineMode.InsertAndIgnoreDuplicate ? "WITH (IGNORE_DUP_KEY = ON)" : "")};");
				}
			}

			var sql = builder.ToString();
			return sql;
		}

		private string GenerateColumn(Column column, bool isPrimary)
		{
			var columnName = IgnoreCase ? column.Name.ToLower() : column.Name;
			var dataType = GetDataTypeSql(column.DataType, column.Length);

			if (isPrimary)
			{
				dataType = $"{dataType} NOT NULL";
			}

			return $"[{columnName}] {dataType}";
		}

		private string GenerateInsertSql(TableInfo tableInfo)
		{
			var columns = tableInfo.Columns;
			var isAutoIncrementPrimary = tableInfo.IsAutoIncrementPrimary;
			// 去掉自增主键
			var insertColumns =
				(isAutoIncrementPrimary ? columns.Where(c1 => c1.Name != tableInfo.Primary.First().Name) : columns)
				.ToArray();

			var columnsSql = string.Join(", ",
				insertColumns.Select(p => $"[{(IgnoreCase ? p.Name.ToLower() : p.Name)}]"));

			if (AutoTimestamp)
			{
				columnsSql = $"{columnsSql}, [creation_time], [creation_date]";
			}

			string columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Name}"));

			if (AutoTimestamp)
			{
				columnsParamsSql = $"{columnsParamsSql}, GETDATE(), GETDATE()";
			}

			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var sql = string.IsNullOrWhiteSpace(database)
				? $"INSERT INTO [{tableName}] ({columnsSql}) VALUES ({columnsParamsSql});"
				: $"USE {database}; INSERT INTO [{tableName}] ({columnsSql}) VALUES ({columnsParamsSql});";
			return sql;
		}

		private string GenerateUpdateSql(TableInfo tableInfo)
		{
			// 无主键, 无更新字段都无法生成更新SQL
			if (tableInfo.Updates.Count == 0)
			{
				Logger?.LogWarning("Can't generate update sql, the count of update columns is zero.");

				return null;
			}

			if (tableInfo.Primary.Count == 0)
			{
				Logger?.LogWarning("Can't generate update sql, primary key is missing.");
				return null;
			}

			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var where = "";
			foreach (var column in tableInfo.Primary)
			{
				where += $" [{GetColumnName(column)}] = @{column.Name} AND";
			}

			where = where.Substring(0, where.Length - 3);

			var setCols = string.Join(", ", tableInfo.Updates.Select(c => $"[{GetColumnName(c)}]=@{c.Name}"));
			var sql = string.IsNullOrWhiteSpace(database)
				? $"UPDATE [{tableName}] SET {setCols} WHERE {where};"
				: $"USE [{database}]; UPDATE [{tableName}] SET {setCols} WHERE {where};";
			return sql;
		}

		private string GenerateSelectSql(TableInfo tableInfo)
		{
			if (tableInfo.Primary.Count == 0)
			{
				Logger?.LogWarning("Can't generate select sql, primary key is missing.");
				return null;
			}

			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var where = "";
			foreach (var column in tableInfo.Primary)
			{
				where += $" [{GetColumnName(column)}] = @{column.Name} AND";
			}

			where = where.Substring(0, where.Length - 3);

			var sql = string.IsNullOrWhiteSpace(database)
				? $"SELECT * FROM [{tableName}] WHERE {where};"
				: $"USE [{database}]; SELECT * FROM [{tableName}] WHERE {where};";
			return sql;
		}

		private string GetDataTypeSql(DataType type, int length)
		{
			string dataType;

			switch (type)
			{
				case DataType.Bool:
				{
					dataType = "BIT";
					break;
				}
				case DataType.DateTime:
				{
					dataType = "DATETIME DEFAULT(GETDATE())";
					break;
				}
				case DataType.Date:
				{
					dataType = "DATE DEFAULT(GETDATE())";
					break;
				}
				case DataType.Decimal:
				{
					dataType = "DECIMAL(18,2)";
					break;
				}
				case DataType.Double:
				{
					dataType = "FLOAT";
					break;
				}
				case DataType.Float:
				{
					dataType = "FLOAT";
					break;
				}
				case DataType.Int:
				{
					dataType = "INT";
					break;
				}
				case DataType.Long:
				{
					dataType = "BIGINT";
					break;
				}
				default:
				{
					dataType = length <= 0 || length >= 8000 ? "NVARCHAR(MAX)" : $"NVARCHAR({length})";
					break;
				}
			}

			return dataType;
		}

		protected override IDbConnection CreateDbConnection(string connectString)
		{
			return new SqlConnection(connectString);
		}

		protected override SqlStatements GenerateSqlStatements(TableInfo model)
		{
			if (PipelineMode == PipelineMode.InsertNewAndUpdateOld)
			{
				throw new NotImplementedException("Sql Server not support InsertNewAndUpdateOld yet.");
			}

			var sqlStatements = new SqlStatements
			{
				InsertSql = GenerateInsertSql(model),
				InsertAndIgnoreDuplicateSql = GenerateInsertSql(model),
				UpdateSql = GenerateUpdateSql(model),
				SelectSql = GenerateSelectSql(model)
			};

			return sqlStatements;
		}

		protected override void InitDatabaseAndTable(IDbConnection conn, TableInfo model)
		{
			var serverVersion = ((DbConnection) conn).ServerVersion;
			var sql = GenerateIfDatabaseExistsSql(model, serverVersion);

			if (Convert.ToInt16(conn.MyExecuteScalar(sql)) == 0)
			{
				sql = GenerateCreateDatabaseSql(model, serverVersion);
				conn.MyExecute(sql);
			}

			sql = GenerateCreateTableSql(model);
			conn.MyExecute(sql);
		}
	}
}