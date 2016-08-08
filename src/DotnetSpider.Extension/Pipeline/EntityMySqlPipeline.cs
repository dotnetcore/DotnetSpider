using System.Data.Common;
using System.Linq;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Extension.ORM;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using DotnetSpider.Extension.Utils;
using DotnetSpider.Extension.Configuration;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityMySqlPipeline : EntityGeneralPipeline
	{
		private string _autoIncrementString = "AUTO_INCREMENT";

		public EntityMySqlPipeline(Schema schema, EntityMetadata entityDefine, string connectString, PipelineMode mode) : base(schema, entityDefine, connectString, mode)
		{
		}

		protected override DbConnection CreateConnection()
		{
			return new MySqlConnection(ConnectString);
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
			string primaryParamenters = string.Join(", ", Primary.Select(p => $"`{p.Name}`=@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				Schema.Database,
				Schema.TableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		protected override string GetCreateTableSql()
		{
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS  `{Schema.Database }`.`{Schema.TableName}`  (");
			string columNames = string.Join(", ", Columns.Select(p => $"`{p.Name}` {ConvertToDbType(p.DataType.ToLower())} "));
			builder.Append(columNames.Substring(0, columNames.Length));
			builder.Append(Primary == null || Primary.Count == 0 ? ",`__id` bigint AUTO_INCREMENT" : "");
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

		private string GetAutoIncrementString(string name)
		{
			return name == AutoIncrement ? _autoIncrementString : "";
		}

		protected override string GetCreateSchemaSql()
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{Schema.Database}` DEFAULT CHARACTER SET utf8mb4 ;";
		}

		protected override DbParameter CreateDbParameter()
		{
			return new MySqlParameter();
		}

		protected override string ConvertToDbType(string datatype)
		{
			var match = RegexUtil.NumRegex.Match(datatype);
			var length = match.Length == 0 ? 0 : int.Parse(match.Value);

			if (RegexUtil.StringTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "varchar(100)" : $"varchar({length})";
			}

			if ("date" == datatype)
			{
				return "date ";
			}

			if ("bool" == datatype)
			{
				return "tinyint(1) ";
			}

			if ("time" == datatype)
			{
				return "timestamp";
			}

			if ("text" == datatype)
			{
				return "text";
			}

			throw new SpiderException("Unsport datatype: " + datatype);
		}
	}
}
