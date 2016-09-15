using System.Data.Common;
using System.Data.SqlClient;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Common.Sql.MSSql;

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

		protected override DbParameter CreateDbParameter()
		{
			return new SqlParameter();
		}

		protected override string GetCreateSchemaSql()
		{
			return $"use master; if not exists(select * from sysdatabases where name='{Schema.Database}') create database {Schema.Database};use {Schema.Database}";
		}

		protected override string GetCreateTableSql()
		{
			var table = SqlCreatorCreator.CreateTable(Schema.Database, Schema.TableName, true);
			foreach (var col in Columns)
			{
				table.AddColumn(col.Name, ConvertToDbType(col.DataType));
			}
			if (Primary == null || Primary.Count == 0)
			{
				table.AddColumn("__id", "int", "IDENTITY(1,1)", "", false);
			}
			return table.ToCommand().GetStatement();
		}

		protected override string GetInsertSql()
		{
			var table = SqlCreatorCreator.Insert(Schema.Database, Schema.TableName);
			foreach (var col in Columns)
			{
				table.Values(col.Name, $"@{col.Name}");
			}
			return table.ToCommand().GetStatement();
		}

		protected override string GetUpdateSql()
		{
			var table = SqlCreatorCreator.Update(Schema.Database, Schema.TableName);
			foreach (var col in UpdateColumns)
			{
				table.Set(col.Name, $"@{col.Name}");
			}
			foreach (var col in Primary)
			{
				table.Where(col.Name, $"@{col.Name}");
			}
			return table.ToCommand().GetStatement();
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