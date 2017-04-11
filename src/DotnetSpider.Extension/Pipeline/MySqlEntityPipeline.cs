using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Extension.Pipeline
{
	public class MySqlEntityPipeline : BaseEntityDbPipeline
	{
		public MySqlEntityPipeline()
		{
		}

		public MySqlEntityPipeline(string connectString, PipelineMode mode = PipelineMode.Insert, bool checkIfSaveBeforeUpdate = false) : base(connectString, mode, checkIfSaveBeforeUpdate)
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

		protected override string GetInsertSql()
		{
			string columNames = string.Join(", ", Columns.Select(p => $"`{p.Name}`"));
			string values = string.Join(", ", Columns.Select(p => $"@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT IGNORE INTO `{0}`.`{1}` {2} {3};",
				Schema.Database,
				Schema.TableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GetUpdateSql()
		{
			string setParamenters = string.Join(", ", UpdateColumns.Select(p => $"`{p.Name}`=@{p.Name}"));
			string primaryParamenters = string.Join(" AND ", Primary.Select(p => $"`{p.Name}`=@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				Schema.Database,
				Schema.TableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GetSelectSql()
		{
			string selectParamenters = string.Join(", ", UpdateColumns.Select(p => $"`{p.Name}`"));
			string primaryParamenters = string.Join(" AND ", Primary.Select(p => $"`{p.Name}`=@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT {0} FROM `{1}`.`{2}` WHERE {3};",
				selectParamenters,
				Schema.Database,
				Schema.TableName,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GetCreateTableSql()
		{
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS  `{Schema.Database }`.`{Schema.TableName}`  (");
			string columNames = string.Join(", ", Columns.Select(p => $"`{p.Name}` {ConvertToDbType(p.DataType)} "));
			builder.Append(columNames);
			builder.Append(Primary == null || Primary.Count == 0 ? (string.IsNullOrEmpty(columNames) ? "" : ",") + "`__id` bigint AUTO_INCREMENT" : "");
			foreach (var index in Indexs)
			{
				string name = string.Join("_", index.Select(c => c));
				string indexColumNames = string.Join(", ", index.Select(c => $"`{c}`"));
				builder.Append($", KEY `index_{name}` ({indexColumNames.Substring(0, indexColumNames.Length)})");
			}

			foreach (var unique in Uniques)
			{
				string name = string.Join("_", unique.Select(c => c));
				string uniqueColumNames = string.Join(", ", unique.Select(c => $"`{c}`"));
				builder.Append($", UNIQUE KEY `unique_{name}` ({uniqueColumNames.Substring(0, uniqueColumNames.Length)})");
			}

			string primaryColumNames = Primary == null || Primary.Count == 0 ? "`__id` " : string.Join(", ", Primary.Select(c => $"`{c.Name}`"));
			builder.Append($", PRIMARY KEY ({primaryColumNames.Substring(0, primaryColumNames.Length)})");

			builder.Append(") ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8");
			string sql = builder.ToString();
			return sql;
		}

		protected override string GetCreateSchemaSql()
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{Schema.Database}` DEFAULT CHARACTER SET utf8mb4 ;";
		}

		protected override string GetIfSchemaExistsSql()
		{
			return $"SELECT COUNT(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{Schema.Database}';";
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			return new MySqlParameter(name, value);
		}

		protected override string ConvertToDbType(string dataType)
		{
			var match = RegexUtil.NumRegex.Match(dataType);
			var length = match.Length == 0 ? 0 : int.Parse(match.Value);

			if (dataType.StartsWith("STRING,"))
			{
				return length == 0 ? "VARCHAR(100)" : $"VARCHAR({length})";
			}

			if ("DATE" == dataType)
			{
				return "DATE ";
			}

			if ("BOOL" == dataType)
			{
				return "TINYINT(1) ";
			}

			if ("TIME" == dataType)
			{
				return "TIMESTAMP ";
			}

			if ("TEXT" == dataType)
			{
				return "TEXT";
			}

			throw new SpiderException("UNSPORT datatype: " + dataType);
		}

		public override BaseEntityPipeline Clone()
		{
			return new MySqlEntityPipeline(ConnectString, Mode)
			{
				UpdateConnectString = UpdateConnectString
			};
		}
	}
}
