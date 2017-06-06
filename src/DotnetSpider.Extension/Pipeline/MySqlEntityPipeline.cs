using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetSpider.Core;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class MySqlEntityPipeline : BaseEntityDbPipeline
	{
		public MySqlEntityPipeline(string connectString, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override DbConnection CreateConnection()
		{
			for (int i = 0; i < 5; ++i)
			{
				try
				{
					if (string.IsNullOrEmpty(ConnectString))
					{
						ConnectString = UpdateConnectString.GetNew();
					}
					var conn = new MySqlConnection(ConnectString);
					conn.Open();
					return conn;
				}
				catch (Exception e)
				{
					// mysql authentication error
					if (e.Message.ToLower().StartsWith("authentication to host"))
					{
						Thread.Sleep(1000);
						ConnectString = UpdateConnectString.GetNew();
					}
					else
					{
						throw;
					}
				}
			}

			throw new SpiderException("Can't get any connect string.");
		}

		protected override string GetInsertSql(EntityDbMetadata metadata)
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

		protected override string GetUpdateSql(EntityDbMetadata metadata)
		{
			string setParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"`{p}`=@{p}"));

			StringBuilder primaryParamenters = new StringBuilder();
			if ("__id" == metadata.Table.Primary)
			{
				primaryParamenters.Append("`__Id` = @__Id,");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					if (columns.Last() != column)
					{
						primaryParamenters.Append($" `{column}` = @{column} AND ");
					}
					else
					{
						primaryParamenters.Append($" `{column}` = @{column}");
					}
				}
			}
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				metadata.Table.Database,
				metadata.Table.Name,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GetSelectSql(EntityDbMetadata metadata)
		{
			string selectParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"`{p}`"));
			StringBuilder primaryParamenters = new StringBuilder();
			//string.Join(" AND ", $"`{Schema.Primary}`=@{Schema.Primary}");
			if ("__id" == metadata.Table.Primary)
			{
				primaryParamenters.Append("`__Id` = @__Id,");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					if (columns.Last() != column)
					{
						primaryParamenters.Append($" `{column}` = @{column} AND ");
					}
					else
					{
						primaryParamenters.Append($" `{column}` = @{column}");
					}
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

		protected override string GetCreateTableSql(EntityDbMetadata metadata)
		{
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{metadata.Table.Database }`.`{metadata.Table.Name}` (");
			string columNames = string.Join(", ", metadata.Columns.Select(p => $"`{p.Name}` {GetDataTypeSql(p)} "));
			builder.Append(columNames);
			builder.Append(",`cdate` timestamp NULL DEFAULT CURRENT_TIMESTAMP");
			if (metadata.Table.Primary.ToLower() == "__id")
			{
				builder.Append(", `__id` bigint AUTO_INCREMENT");
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

			builder.Append(") ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8");
			string sql = builder.ToString();
			return sql;
		}

		protected override string GetCreateSchemaSql(EntityDbMetadata metadata, string serverVersion)
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{metadata.Table.Database}` DEFAULT CHARACTER SET utf8mb4 ;";
		}

		protected override string GetIfSchemaExistsSql(EntityDbMetadata metadata, string serverVersion)
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
	}
}
