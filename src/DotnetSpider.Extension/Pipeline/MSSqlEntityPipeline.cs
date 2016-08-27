using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DotnetSpider.Extension.Pipeline
{
    using Core;
    using Core.Common;
    using SQL = DotnetSpider.Extension.SQL.MSSql.SQL;
    public class MSSqlEntityPipeline : BaseEntityDbPipeline
    {
        public MSSqlEntityPipeline()
        { }

        public MSSqlEntityPipeline(string connectString, PipelineMode mode = PipelineMode.Insert) : base(connectString, mode)
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
            return new SqlConnection(ConnectString);
        }

        protected override DbParameter CreateDbParameter()
        {
            return new SqlParameter();
        }

        protected override string GetCreateSchemaSql()
        {
            return string.Empty;
        }

        protected override string GetCreateTableSql()
        {
            var table = SQL.CreateTable(Schema.TableName, true);
            foreach (var col in Columns)
            {
                table.AddColumn(col.Name, ConvertToDbType(col.DataType));
            }
            if (Primary == null || Primary.Count == 0)
            {
                table.AddColumn("__id", "int", "IDENTITY(1,1)", "", false);
            }
            return table.toCommand().getStatement();
        }

        protected override string GetInsertSql()
        {
            var table = SQL.INSERT(Schema.TableName);
            foreach (var col in Columns)
            {
                table.Values(col.Name, $"@{col.Name}");
            }
            return table.toCommand().getStatement();
        }

        protected override string GetUpdateSql()
        {
            var table = SQL.UPDATE(Schema.TableName);
            foreach (var col in UpdateColumns)
            {
                table.Set(col.Name, $"@{col.Name}");
            }
            foreach (var col in Primary)
            {
                table.Where(col.Name, $"@{col.Name}");
            }
            return table.toCommand().getStatement();
        }
    }
}
