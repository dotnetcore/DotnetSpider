using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using DotnetSpider.Extension.Model;
using System.Configuration;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public class SqlServerEntityPipeline : BaseEntityDbPipeline
	{
		public SqlServerEntityPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			if (value == null)
			{
				value = DBNull.Value;
			}
			return new SqlParameter(name, value);
		}

		protected override string GenerateCreateDatabaseSql(EntityAdapter adapter, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{adapter.Table.Database}') CREATE DATABASE {adapter.Table.Database};";
					}
				default:
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{adapter.Table.Database}') CREATE DATABASE {adapter.Table.Database};";
					}
			}
		}

		protected override string GenerateIfDatabaseExistsSql(EntityAdapter adapter, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"SELECT COUNT(*) FROM sysdatabases WHERE name='{adapter.Table.Database}'";
					}
				default:
					{
						return $"SELECT COUNT(*) FROM sys.databases WHERE name='{adapter.Table.Database}'";
					}
			}
		}

		protected override string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder builder = new StringBuilder($"USE {adapter.Table.Database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");
			StringBuilder columnNames = new StringBuilder();

			foreach (var p in adapter.Columns)
			{
				columnNames.Append($",[{p.Name}] {GetDataTypeSql(p)}");
			}

			builder.Append(columnNames.ToString().Substring(1, columnNames.Length - 1));
			builder.Append(",[CDate] DATETIME DEFAULT(GETDATE())");

			if (Core.Environment.IdColumn.ToLower() == adapter.Table.Primary.ToLower())
			{
				builder.Append($", [{Core.Environment.IdColumn}] [bigint] IDENTITY(1,1) NOT NULL");
			}

			builder.Append(",");
			StringBuilder primaryKey = new StringBuilder();
			if (string.IsNullOrEmpty(adapter.Table.Primary))
			{
				primaryKey.Append($"[{Core.Environment.IdColumn}] ASC,");
			}
			else
			{
				var columns = adapter.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryKey.Append($"[{column}] ASC,");
				}
			}

			builder.Append(
				$" CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({primaryKey.ToString(0, primaryKey.Length - 1)})WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");

			if (adapter.Table.Indexs != null)
			{
				foreach (var index in adapter.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE NONCLUSTERED INDEX [index_{name}] ON {tableName} ({indexColumNames.Substring(0, indexColumNames.Length)});");
				}
			}

			if (adapter.Table.Uniques != null)
			{
				foreach (var unique in adapter.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE UNIQUE NONCLUSTERED INDEX [unique_{name}] ON {tableName} ({uniqueColumNames.Substring(0, uniqueColumNames.Length)});");
				}
			}
			return builder.ToString();
		}

		protected override string GenerateInsertSql(EntityAdapter adapter)
		{
			string columNames = string.Join(", ", adapter.Columns.Select(p => $"[{p.Name}]"));
			string values = string.Join(", ", adapter.Columns.Select(p => $"@{p.Name}"));
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; INSERT INTO [{1}] {2} {3};",
				adapter.Table.Database,
				tableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GenerateUpdateSql(EntityAdapter adapter)
		{
			string setParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"[{p}]=@{p}"));
			StringBuilder primaryParamenters = new StringBuilder();
			if (string.IsNullOrEmpty(adapter.Table.Primary))
			{
				primaryParamenters.Append("[__Id] = @__Id");
			}
			else
			{
				var columns = adapter.Table.Primary.Split(',');
				foreach (var column in columns)
				{
					primaryParamenters.Append(columns.Last() != column ? $" [{column}] = @{column} AND " : $" [{column}] = @{column}");
				}
			}

			var sqlBuilder = new StringBuilder();
			var tableName = adapter.Table.CalculateTableName();
			sqlBuilder.AppendFormat("USE {0}; UPDATE [{1}] SET {2} WHERE {3};",
				adapter.Table.Database,
				tableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GenerateSelectSql(EntityAdapter adapter)
		{
			string selectParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"[{p}]"));
			string primaryParamenters = $" [{adapter.Table.Primary}]=@{adapter.Table.Primary}";

			var sqlBuilder = new StringBuilder();
			var tableName = adapter.Table.CalculateTableName();
			sqlBuilder.AppendFormat("USE {0}; SELECT {1} FROM [{2}] WHERE {3};",
				adapter.Table.Database,
				selectParamenters,
				tableName,
				primaryParamenters);

			return sqlBuilder.ToString();
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
				dataType = "DATETIME";
			}
			else if (field.DataType == DataTypeNames.Decimal)
			{
				dataType = "DECIMAL(18,2)";
			}
			else if (field.DataType == DataTypeNames.Double)
			{
				dataType = "FLOAT";
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
				dataType = field.Length <= 0 ? "NVARCHAR(MAX)" : $"NVARCHAR({field.Length}) {(field.NotNull ? "NOT NULL" : "NULL")}";
			}

			return dataType;
		}

		protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
		{
			ConnectionStringSettings connectionStringSettings;
			if (!string.IsNullOrEmpty(connectString))
			{
				connectionStringSettings = new ConnectionStringSettings("SqlServer", connectString, "System.Data.SqlClient");
			}
			else
			{
				if (Core.Environment.DataConnectionStringSettings != null)
				{
					connectionStringSettings = Core.Environment.DataConnectionStringSettings;
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