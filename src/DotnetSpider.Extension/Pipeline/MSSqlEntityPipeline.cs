using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using System.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class MsSqlEntityPipeline : BaseEntityDbPipeline
	{
		public MsSqlEntityPipeline()
		{ }

		public MsSqlEntityPipeline(string connectString, PipelineMode mode = PipelineMode.Insert) : base(connectString, mode)
		{
		}

		protected override string ConvertToDbType(string dataType)
		{
			var match = RegexUtil.NumRegex.Match(dataType);
			var length = match.Length == 0 ? 0 : int.Parse(match.Value);

			if (dataType.StartsWith("STRING,"))
			{
				return length == 0 ? "NVARCHAR(100)" : $"NVARCHAR({length})";
			}

			if ("DATE" == dataType)
			{
				return "datetime ";
			}

			if ("BOOL" == dataType)
			{
				return "bit ";
			}

			if ("TIME" == dataType)
			{
				return "datetime ";
			}

			if ("TEXT" == dataType)
			{
				return "nvarchar(max)";
			}

			throw new SpiderException("UNSPORT datatype: " + dataType);
		}

		protected override DbConnection CreateConnection()
		{
			var conn = new SqlConnection(ConnectString);
			conn.Open();
			return conn;
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			return new SqlParameter(name, value ?? DBNull.Value);
		}

		protected override string GetCreateSchemaSql()
		{
			return $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{Schema.Database}') CREATE DATABASE {Schema.Database}; USE {Schema.Database};";
		}

		protected override string GetCreateTableSql()
		{
			var identity = "IDENTITY(1,1)";
			StringBuilder builder = new StringBuilder($"USE {Schema.Database}; IF OBJECT_ID('{Schema.TableName}', 'U') IS NULL CREATE table {Schema.TableName} (");
			StringBuilder columnNames = new StringBuilder();
			if (Primary.Count == 0)
			{
				foreach (var p in Columns)
				{
					columnNames.Append($",[{p.Name}] {ConvertToDbType(p.DataType)} NULL");
				}
			}
			else
			{
				foreach (var p in Columns)
				{
					var identityPart = AutoIncrement.Contains(p.Name) ? identity : "";
					var nullPart = Primary.Any(k => k.Name == p.Name) ? "NOT NULL" : "NULL";

					columnNames.Append($",[{p.Name}] {ConvertToDbType(p.DataType)} {identityPart} {nullPart}");
				}
			}

			builder.Append(columnNames.ToString().Substring(1, columnNames.Length - 1));
			builder.Append(Primary == null || Primary.Count == 0 ? (columnNames.Length == 0 ? "" : ",") + "[__id] [int] IDENTITY(1,1) NOT NULL," : ",");
			string primaryKey = Primary == null || Primary.Count == 0 ? "[__id] ASC" : string.Join(", ", Primary.Select(p => $"[{p.Name}] ASC"));
			builder.Append(
				$" CONSTRAINT [PK_{Schema.TableName}] PRIMARY KEY CLUSTERED ({primaryKey.Substring(0, primaryKey.Length)})WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");

			foreach (var index in Indexs)
			{
				string name = string.Join("_", index.Select(c => c));
				string indexColumNames = string.Join(", ", index.Select(c => $"[{c}]"));
				builder.Append($"CREATE NONCLUSTERED INDEX [index_{name}] ON {Schema.TableName} ({indexColumNames.Substring(0, indexColumNames.Length)});");
			}

			foreach (var unique in Uniques)
			{
				string name = string.Join("_", unique.Select(c => c));
				string uniqueColumNames = string.Join(", ", unique.Select(c => $"[{c}]"));
				builder.Append($"CREATE UNIQUE NONCLUSTERED INDEX [unique_{name}] ON {Schema.TableName} ({uniqueColumNames.Substring(0, uniqueColumNames.Length)});");
			}
			return builder.ToString();
		}

		protected override string GetInsertSql()
		{
			string columNames = string.Join(", ", Columns.Select(p => $"[{p.Name}]"));
			string values = string.Join(", ", Columns.Select(p => $"@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; INSERT INTO [{1}] {2} {3};",
				Schema.Database,
				Schema.TableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GetUpdateSql()
		{
			string setParamenters = string.Join(", ", UpdateColumns.Select(p => $"[{p.Name}]=@{p.Name}"));
			string primaryParamenters = string.Join(" AND ", Primary.Select(p => $"[{p.Name}]=@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("USE {0}; UPDATE [{1}] SET {2} WHERE {3};",
				Schema.Database,
				Schema.TableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		public override BaseEntityPipeline Clone()
		{
			return new MsSqlEntityPipeline(ConnectString, Mode)
			{
				UpdateConnectString = UpdateConnectString
			};
		}
	}
}