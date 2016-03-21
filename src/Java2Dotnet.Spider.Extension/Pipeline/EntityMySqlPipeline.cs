using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityMySqlPipeline : EntityGeneralPipeline
	{
		private string _autoIncrementString = "AUTO_INCREMENT";

		public EntityMySqlPipeline(Schema schema, JObject entityDefine, string connectString) : base(schema, entityDefine, connectString)
		{
		}

		protected override DbConnection CreateConnection()
		{
			return new MySqlConnection(ConnectString);
		}

		protected override string GetInsertSql()
		{
			string columNames = string.Join(", ", Columns.Select(p => $"{p.Name}"));
			string values = string.Join(", ", Columns.Select(p => $"@{p.Name}"));

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT IGNORE INTO `{0}`.`{1}` {2} {3};",
				Schema.Database,
				Schema.TableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			return sqlBuilder.ToString();
		}

		protected override string GetCreateTableSql()
		{
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS  `{Schema.Database }`.`{Schema.TableName}`  (");
			string columNames = string.Join(", ", Columns.Select(p => $"`{p.Name}` {ConvertToDbType(p.DataType.ToLower())} {GetAutoIncrementString(p.Name)}"));
			builder.Append(columNames.Substring(0, columNames.Length));

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


			string primaryColumNames = string.Join(", ", Primary.Select(c => $"`{c}`"));
			builder.Append($", PRIMARY KEY ({primaryColumNames.Substring(0, primaryColumNames.Length)})");

			builder.Append(") ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8");
            string sql=builder.ToString();
            Logger.Info(sql);
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

		protected override string ConvertToDbType(string datatype)
		{
			var match = RegexUtil.NumRegex.Match(datatype);
			var length = match.Length == 0 ? 0 : int.Parse(match.Value);

			if (RegexUtil.StringTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "varchar(100)" : $"varchar({length})";
			}

			if (RegexUtil.IntTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "int(11)" : $"int({length})";
			}

			if (RegexUtil.BigIntTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "bigint(11)" : $"bigint({length})";
			}

			if (RegexUtil.FloatTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "float(11)" : $"float({length})";
			}

			if (RegexUtil.DateTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "date " : $"date({length})";
			}

			if (RegexUtil.TimeStampTypeRegex.IsMatch(datatype))
			{
				length = length > 6 ? 6 : length;
				//return length == 0 ? "timestamp " : $"timestamp({length})";
                // mysql5.5 not support set length                
                return "timestamp";
			}

			if (RegexUtil.DoubleTypeRegex.IsMatch(datatype))
			{
				return length == 0 ? "double(11)" : $"double({length})";
			}

			if ("text" == datatype)
			{
				return "text";
			}

			throw new SpiderExceptoin("Unsport datatype: " + datatype);
		}
	}
}
