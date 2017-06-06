using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class SqlServerEntityPipeline : BaseEntityDbPipeline
	{
		public SqlServerEntityPipeline(string connectString, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override DbConnection CreateConnection()
		{
			var conn = new SqlConnection(ConnectString);
			conn.Open();
			return conn;
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			if (value == null)
			{
				value = DBNull.Value;
			}
			return new SqlParameter(name, value);
		}

		protected override string GetCreateSchemaSql(EntityDbMetadata metadata, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{metadata.Table.Database}') CREATE DATABASE {metadata.Table.Database};";
					}
				default:
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{metadata.Table.Database}') CREATE DATABASE {metadata.Table.Database};";
					}
			}
		}

		protected override string GetIfSchemaExistsSql(EntityDbMetadata metadata, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"SELECT COUNT(*) FROM sysdatabases WHERE name='{metadata.Table.Database}'";
					}
				default:
					{
						return $"SELECT COUNT(*) FROM sys.databases WHERE name='{metadata.Table.Database}'";
					}
			}
		}

		protected override string GetCreateTableSql(EntityDbMetadata metadata)
		{
			StringBuilder builder = new StringBuilder($"USE {metadata.Table.Database}; IF OBJECT_ID('{metadata.Table.Name}', 'U') IS NULL CREATE table {metadata.Table.Name} (");
			StringBuilder columnNames = new StringBuilder();

			foreach (var p in metadata.Columns)
			{
				columnNames.Append($",[{p.Name}] {GetDataTypeSql(p)}");
			}

			builder.Append(columnNames.ToString().Substring(1, columnNames.Length - 1));
			builder.Append(",[CDate] DATETIME DEFAULT(GETDATE())");

			if ("__id" == metadata.Table.Primary.ToLower())
			{
				builder.Append(", [__Id] [bigint] IDENTITY(1,1) NOT NULL");
			}

			builder.Append(",");
			StringBuilder primaryKey = new StringBuilder();
			if (string.IsNullOrEmpty(metadata.Table.Primary))
			{
				primaryKey.Append("[__Id] ASC,");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryKey.Append($"[{column}] ASC,");
				}
			}

			builder.Append(
				$" CONSTRAINT [PK_{metadata.Table.Name}] PRIMARY KEY CLUSTERED ({primaryKey.ToString(0, primaryKey.Length - 1)})WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");

			if (metadata.Table.Indexs != null)
			{
				foreach (var index in metadata.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE NONCLUSTERED INDEX [index_{name}] ON {metadata.Table.Name} ({indexColumNames.Substring(0, indexColumNames.Length)});");
				}
			}

			if (metadata.Table.Uniques != null)
			{
				foreach (var unique in metadata.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE UNIQUE NONCLUSTERED INDEX [unique_{name}] ON {metadata.Table.Name} ({uniqueColumNames.Substring(0, uniqueColumNames.Length)});");
				}
			}
			return builder.ToString();
		}

		protected override string GetInsertSql(EntityDbMetadata metadata)
		{
			string columNames = string.Join(", ", metadata.Columns.Select(p => $"[{p.Name}]"));
			string values = string.Join(", ", metadata.Columns.Select(p => $"@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; INSERT INTO [{1}] {2} {3};",
				metadata.Table.Database,
				metadata.Table.Name,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GetUpdateSql(EntityDbMetadata metadata)
		{
			string setParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"[{p}]=@{p}"));
			StringBuilder primaryParamenters = new StringBuilder();
			if (string.IsNullOrEmpty(metadata.Table.Primary))
			{
				primaryParamenters.Append("[__Id] = @__Id,");
			}
			else
			{
				var columns = metadata.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					if (columns.Last() != column)
					{
						primaryParamenters.Append($" [{column}] = @{column} AND ");
					}
					else
					{
						primaryParamenters.Append($" [{column}] = @{column}");
					}
				}
			}

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; UPDATE [{1}] SET {2} WHERE {3};",
				metadata.Table.Database,
				metadata.Table.Name,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GetSelectSql(EntityDbMetadata metadata)
		{
			string selectParamenters = string.Join(", ", metadata.Table.UpdateColumns.Select(p => $"[{p}]"));
			string primaryParamenters = $" [{metadata.Table.Primary}]=@{metadata.Table.Primary}";

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; SELECT {1} FROM [{2}] WHERE {3};",
				metadata.Table.Database,
				selectParamenters,
				metadata.Table.Name,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected string GetDataTypeSql(Field field)
		{
			switch (field.DataType)
			{
				case DataType.Bigint:
					{
						return "BIGINT";
					}
				case DataType.Int:
					{
						return "INT";
					}
				case DataType.Double:
					{
						return "FLOAT";
					}
				case DataType.Float:
					{
						return "FLOAT";
					}
				case DataType.Text:
					{
						return field.Length <= 0 ? "NVARCHAR(MAX)" : $"NVARCHAR({field.Length}) {(field.NotNull ? "NOT NULL" : "NULL")}";
					}
				case DataType.Time:
					{
						return "DATETIME";
					}
			}
			return "TEXT";
		}
	}
}