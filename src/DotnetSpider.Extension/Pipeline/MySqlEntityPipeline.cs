using System.Linq;
using System.Text;
using DotnetSpider.Extension.Infrastructure;
using System.Data;
using MySql.Data.MySqlClient;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model;
using Microsoft.Extensions.Logging;
using DotnetSpider.Extension.Model;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到MySql中
	/// </summary>
	public class MySqlEntityPipeline : DbEntityPipeline
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串, 如果为空框架会尝试从配置文件中读取</param>
		/// <param name="pipelineMode">数据管道模式</param>
		public MySqlEntityPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate) : base(connectString, pipelineMode)
		{
		}

		protected override IDbConnection CreateDbConnection(string connectString)
		{
			return new MySqlConnection(connectString);
		}

		protected override Sqls GenerateSqls(TableInfo tableInfo)
		{
			var sqls = new Sqls
			{
				InsertSql = GenerateInsertSql(tableInfo, false),
				InsertAndIgnoreDuplicateSql = GenerateInsertSql(tableInfo, true),
				InsertNewAndUpdateOldSql = GenerateInsertNewAndUpdateOldSql(tableInfo),
				UpdateSql = GenerateUpdateSql(tableInfo),
				SelectSql = GenerateSelectSql(tableInfo)
			};
			return sqls;
		}

		protected override void InitDatabaseAndTable(IDbConnection conn, TableInfo tableInfo)
		{
			var database = GetDatabaseName(tableInfo);
			if (!string.IsNullOrWhiteSpace(database))
			{
				conn.MyExecute($"CREATE SCHEMA IF NOT EXISTS `{database}` DEFAULT CHARACTER SET utf8mb4;");
			}
			conn.MyExecute(GenerateCreateTableSql(tableInfo));
		}

		/// <summary>
		/// 构造创建数据表的SQL语句
		/// </summary>
		/// <param name="model">数据模型</param>
		/// <returns>SQL语句</returns>
		private string GenerateCreateTableSql(TableInfo tableInfo)
		{
			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var isAutoIncrementPrimary = tableInfo.IsAutoIncrementPrimary;

			StringBuilder builder = string.IsNullOrWhiteSpace(database) ? new StringBuilder($"CREATE TABLE IF NOT EXISTS `{tableName}` (") :
				new StringBuilder($"CREATE TABLE IF NOT EXISTS `{database}`.`{tableName}` (");

			foreach (var column in tableInfo.Columns)
			{
				var isPrimary = tableInfo.Primary.Any(k => k.Name == column.Name);

				var columnSql = GenerateColumn(column, isPrimary);

				if (isAutoIncrementPrimary && isPrimary)
				{
					builder.Append($"{columnSql} AUTO_INCREMENT, ");
				}
				else
				{
					builder.Append($"{columnSql}, ");
				}
			}
			builder.Remove(builder.Length - 2, 2);

			if (AutoTimestamp)
			{
				builder.Append($", `creation_time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP, `creation_date` DATE");
			}

			if (tableInfo.Primary.Count > 0)
			{
				builder.Append($", PRIMARY KEY ({string.Join(", ", tableInfo.Primary.Select(c => $"`{GetColumnName(c)}`"))})");
			}

			if (tableInfo.Indexes.Count > 0)
			{
				foreach (var index in tableInfo.Indexes)
				{
					string name = index.Key;
					string indexColumNames = string.Join(", ", index.Value.Select(c => $"`{GetColumnName(c)}`"));
					builder.Append($", KEY `INDEX_{name}` ({indexColumNames})");
				}
			}
			if (tableInfo.Uniques.Count > 0)
			{
				foreach (var unique in tableInfo.Uniques)
				{
					string name = unique.Key;
					string uniqueColumNames = string.Join(", ", unique.Value.Select(c => $"`{GetColumnName(c)}`"));
					builder.Append($", UNIQUE KEY `UNIQUE_{name}` ({uniqueColumNames})");
				}
			}
			builder.Append(")");
			string sql = builder.ToString();
			return sql;
		}

		private string GenerateInsertSql(TableInfo tableInfo, bool ignoreDuplicate)
		{
			var columns = tableInfo.Columns;
			var isAutoIncrementPrimary = tableInfo.IsAutoIncrementPrimary;
			// 去掉自增主键
			var insertColumns = isAutoIncrementPrimary ? columns.Where(c1 => c1.Name != tableInfo.Primary.First().Name) : columns;

			string columnsSql = string.Join(", ", insertColumns.Select(c => $"`{GetColumnName(c)}`"));

			if (AutoTimestamp)
			{
				columnsSql = $"{columnsSql},`creation_time`, `creation_date`";
			}
			string columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Name}"));

			if (AutoTimestamp)
			{
				columnsParamsSql = $"{columnsParamsSql}, NOW(), CURRENT_DATE()";
			}

			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			var sql = string.IsNullOrWhiteSpace(database) ?
				$"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO `{tableName}` ({columnsSql}) VALUES ({columnsParamsSql});" :
				$"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO `{database}`.`{tableName}` ({columnsSql}) VALUES ({columnsParamsSql});";
			return sql;
		}

		private string GenerateInsertNewAndUpdateOldSql(TableInfo tableInfo)
		{
			var columns = tableInfo.Columns;
			var isAutoIncrementPrimary = tableInfo.IsAutoIncrementPrimary;
			// 去掉自增主键
			var insertColumns = isAutoIncrementPrimary ? columns.Where(c1 => c1.Name != tableInfo.Primary.First().Name) : columns;

			string columnsSql = string.Join(", ", insertColumns.Select(c => $"`{GetColumnName(c)}`"));

			if (AutoTimestamp)
			{
				columnsSql = $"{columnsSql},`creation_time`, `creation_date`";
			}
			string columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Name}"));

			if (AutoTimestamp)
			{
				columnsParamsSql = $"{columnsParamsSql}, NOW(), CURRENT_DATE()";
			}

			var tableName = GetTableName(tableInfo);
			var database = GetDatabaseName(tableInfo);

			string setParams = string.Join(", ", insertColumns.Select(c => $"`{GetColumnName(c)}`=@{c.Name}"));

			var sql = string.IsNullOrWhiteSpace(database) ? $"INSERT INTO `{tableName}` ({columnsSql}) VALUES ({columnsParamsSql}) ON DUPLICATE KEY UPDATE {setParams};" :
				$"INSERT INTO `{database}`.`{tableName}` ({columnsSql}) VALUES ({columnsParamsSql}) ON DUPLICATE KEY UPDATE {setParams};";

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

			string where = "";
			foreach (var column in tableInfo.Primary)
			{
				where += $" `{GetColumnName(column)}` = @{column.Name} AND";
			}
			where = where.Substring(0, where.Length - 3);

			string setCols = string.Join(", ", tableInfo.Updates.Select(c => $"`{GetColumnName(c)}`=@{c.Name}"));
			var sql = string.IsNullOrWhiteSpace(database) ? $"UPDATE `{tableName}` SET {setCols} WHERE {where};" :
				$"UPDATE `{database}`.`{tableName}` SET {setCols} WHERE {where};";
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

			string where = "";
			foreach (var column in tableInfo.Primary)
			{
				where += $" `{GetColumnName(column)}` = @{column.Name} AND";
			}

			var sql = string.IsNullOrWhiteSpace(database) ? $"SELECT * FROM `{tableName}` WHERE {where}" :
				$"SELECT * FROM `{database}`.`{tableName}` WHERE {where}";

			return sql;
		}

		private string GenerateColumn(Column column, bool isPrimary)
		{
			var columnName = GetColumnName(column);
			var dataType = GetDataTypeSql(column.DataType, column.Length);
			if (isPrimary)
			{
				dataType = $"{dataType} NOT NULL";
			}
			return $"`{columnName}` {dataType}";
		}

		private string GetDataTypeSql(DataType type, int length)
		{
			string dataType;

			switch (type)
			{
				case DataType.Bool:
					{
						dataType = "BOOL";
						break;
					}
				case DataType.DateTime:
					{
						dataType = "TIMESTAMP DEFAULT CURRENT_TIMESTAMP";
						break;
					}
				case DataType.Date:
					{
						dataType = "DATE";
						break;
					}
				case DataType.Decimal:
					{
						dataType = "DECIMAL(18,2)";
						break;
					}
				case DataType.Double:
					{
						dataType = "DOUBLE";
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
						dataType = length <= 0 || length > 8000 ? "LONGTEXT" : $"VARCHAR({length})";
						break;
					}
			}
			return dataType;
		}
	}
}
