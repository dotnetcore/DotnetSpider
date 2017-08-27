using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure.Database;
using System.Configuration;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Pipeline
{
	public class MySqlEntityPipeline : BaseEntityDbPipeline
	{
		public MySqlEntityPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override string GenerateInsertSql(EntityDbMetadata metadata)
		{
			string columNames = string.Join(", ", metadata.Columns.Select(p => $"`{p.Name}`"));
			string values = string.Join(", ", metadata.Columns.Select(p => $"@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT IGNORE INTO `{0}`.`{1}` {2} {3};",
				metadata.Table.Database,
				metadata.Table.Name,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GenerateUpdateSql(EntityDbMetadata metadata)
		{
			string setParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"`{p}`=@{p}"));

			StringBuilder primaryParamenters = new StringBuilder();
			if (Core.Environment.IdColumn == metadata.Table.Primary)
			{
				primaryParamenters.Append($"`{Core.Environment.IdColumn}` = @__id");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryParamenters.Append(columns.Last() != column ? $" `{column}` = @{column} AND " : $" `{column}` = @{column}");
				}
			}
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				metadata.Table.Database,
				metadata.Table.Name,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GenerateSelectSql(EntityDbMetadata metadata)
		{
			string selectParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"`{p}`"));
			StringBuilder primaryParamenters = new StringBuilder();

			if (Core.Environment.IdColumn == metadata.Table.Primary)
			{
				primaryParamenters.Append($"`{Core.Environment.IdColumn}` = @{Core.Environment.IdColumn},");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryParamenters.Append(columns.Last() != column ? $" `{column}` = @{column} AND " : $" `{column}` = @{column}");
				}
			}
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT {0} FROM `{1}`.`{2}` WHERE {3};",
				selectParamenters,
				metadata.Table.Database,
				metadata.Table.Name,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GenerateCreateTableSql(EntityDbMetadata metadata)
		{
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{metadata.Table.Database }`.`{metadata.Table.Name}` (");
			string columNames = string.Join(", ", metadata.Columns.Select(p => $"`{p.Name}` {GetDataTypeSql(p)} "));
			builder.Append(columNames);
			builder.Append(",`cdate` timestamp NULL DEFAULT CURRENT_TIMESTAMP");
			if (metadata.Table.Primary.ToLower() == Core.Environment.IdColumn)
			{
				builder.Append($", `{Core.Environment.IdColumn}` bigint AUTO_INCREMENT");
			}

			if (metadata.Table.Indexs != null)
			{
				foreach (var index in metadata.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", KEY `index_{name}` ({indexColumNames.Substring(0, indexColumNames.Length)})");
				}
			}
			if (metadata.Table.Uniques != null)
			{
				foreach (var unique in metadata.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", UNIQUE KEY `unique_{name}` ({uniqueColumNames.Substring(0, uniqueColumNames.Length)})");
				}
			}
			builder.Append($", PRIMARY KEY ({ metadata.Table.Primary})");
			builder.Append(") AUTO_INCREMENT=1");
			string sql = builder.ToString();
			return sql;
		}

		protected override string GenerateCreateDatabaseSql(EntityDbMetadata metadata, string serverVersion)
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{metadata.Table.Database}` DEFAULT CHARACTER SET utf8mb4 ;";
		}

		protected override string GenerateIfDatabaseExistsSql(EntityDbMetadata metadata, string serverVersion)
		{
			return $"SELECT COUNT(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{metadata.Table.Database}';";
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			return new MySqlParameter(name, value);
		}

		protected string GetDataTypeSql(Field field)
		{
			switch (field.DataType)
			{
				case DataType.Bigint:
					{
						return "bigint";
					}
				case DataType.Int:
					{
						return "int";
					}
				case DataType.Double:
					{
						return "double";
					}
				case DataType.Float:
					{
						return "float";
					}
				case DataType.Text:
					{
						return (field.Length <= 0) ? "TEXT" : $"VARCHAR({field.Length})";
					}
				case DataType.Time:
					{
						return "timestamp null";
					}
			}
			return "TEXT";
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
				if (Environment.DataConnectionStringSettings != null)
				{
					connectionStringSettings = Environment.DataConnectionStringSettings;
				}
				else
				{
					throw new SpiderException("DataConnection is unfound in app.config.");
				}
			}
			return connectionStringSettings;
		}
	}
}
