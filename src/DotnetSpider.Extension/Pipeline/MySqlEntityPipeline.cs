using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Model;
using System.Configuration;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public class MySqlEntityPipeline : BaseEntityDbPipeline
	{
		public MySqlEntityPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override string GenerateInsertSql(EntityAdapter adapter)
		{
			string columNames = string.Join(", ", adapter.Columns.Select(p => $"`{p.Name}`"));
			string values = string.Join(", ", adapter.Columns.Select(p => $"@{p.Name}"));
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT IGNORE INTO `{0}`.`{1}` {2} {3};",
				adapter.Table.Database,
				tableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GenerateUpdateSql(EntityAdapter adapter)
		{
			string setParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"`{p}`=@{p}"));
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder primaryParamenters = new StringBuilder();
			if (Environment.IdColumn == adapter.Table.Primary)
			{
				primaryParamenters.Append($"`{Environment.IdColumn}` = @__Id");
			}
			else
			{
				var columns = adapter.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryParamenters.Append(columns.Last() != column ? $" `{column}` = @{column} AND " : $" `{column}` = @{column}");
				}
			}
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				adapter.Table.Database,
				tableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GenerateSelectSql(EntityAdapter adapter)
		{
			string selectParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"`{p}`"));
			StringBuilder primaryParamenters = new StringBuilder();

			if (Environment.IdColumn == adapter.Table.Primary)
			{
				primaryParamenters.Append($"`{Environment.IdColumn}` = @{Environment.IdColumn},");
			}
			else
			{
				var columns = adapter.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryParamenters.Append(columns.Last() != column ? $" `{column}` = @{column} AND " : $" `{column}` = @{column}");
				}
			}
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT {0} FROM `{1}`.`{2}` WHERE {3};",
				selectParamenters,
				adapter.Table.Database,
				tableName,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{adapter.Table.Database }`.`{tableName}` (");
			string columNames = string.Join(", ", adapter.Columns.Select(p => $"`{p.Name}` {GetDataTypeSql(p)} "));
			builder.Append(columNames);
			builder.Append(",`CDate` timestamp NULL DEFAULT CURRENT_TIMESTAMP");
			if (Environment.IdColumn.ToLower() == adapter.Table.Primary.ToLower())
			{
				builder.Append($", `{Environment.IdColumn}` bigint AUTO_INCREMENT");
			}

			if (adapter.Table.Indexs != null)
			{
				foreach (var index in adapter.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", KEY `index_{name}` ({indexColumNames.Substring(0, indexColumNames.Length)})");
				}
			}
			if (adapter.Table.Uniques != null)
			{
				foreach (var unique in adapter.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", UNIQUE KEY `unique_{name}` ({uniqueColumNames.Substring(0, uniqueColumNames.Length)})");
				}
			}
			builder.Append($", PRIMARY KEY ({ adapter.Table.Primary})");
			builder.Append(") AUTO_INCREMENT=1");
			string sql = builder.ToString();
			return sql;
		}

		protected override string GenerateCreateDatabaseSql(EntityAdapter adapter, string serverVersion)
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{adapter.Table.Database}` DEFAULT CHARACTER SET utf8mb4;";
		}

		protected override string GenerateIfDatabaseExistsSql(EntityAdapter adapter, string serverVersion)
		{
			return $"SELECT COUNT(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{adapter.Table.Database}';";
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			var parameter = new MySqlParameter(name, MySqlDbType.String) { Value = value };
			return parameter;
		}

		protected string GetDataTypeSql(Column field)
		{
			var dataType = "TEXT";

			if (field.DataType == DataTypeNames.Boolean)
			{
				dataType = "BOOL";
			}
			else if (field.DataType == DataTypeNames.DateTime)
			{
				dataType = "TIMESTAMP NULL";
			}
			else if (field.DataType == DataTypeNames.Decimal)
			{
				dataType = "DECIMAL(18,2)";
			}
			else if (field.DataType == DataTypeNames.Double)
			{
				dataType = "DOUBLE";
			}
			else if (field.DataType == DataTypeNames.Float)
			{
				dataType = "FLOAT";
			}
			else if (field.DataType == DataTypeNames.Int)
			{
				dataType = "INT";
			}
			else if (field.DataType == DataTypeNames.Int64)
			{
				dataType = "BIGINT";
			}
			else if (field.DataType == DataTypeNames.String)
			{
				dataType = (field.Length <= 0) ? "TEXT" : $"VARCHAR({field.Length})";
			}

			return dataType;
		}

		protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
		{
			ConnectionStringSettings connectionStringSettings;
			if (!string.IsNullOrEmpty(connectString))
			{
				connectionStringSettings = new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
			}
			else
			{
				return null;
			}
			return connectionStringSettings;
		}
	}

	public class MySqlEntityPipeline<T> : MySqlEntityPipeline where T : SpiderEntity
	{
		public MySqlEntityPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
			AddEntity(EntityDefine.Parse<T>());
		}
	}
}
