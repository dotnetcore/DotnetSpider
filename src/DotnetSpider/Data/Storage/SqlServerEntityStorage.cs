using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Data.Storage.Model;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Data.Storage
{
    public enum SqlServerVersion
    {
        V2000,
        V2005,
        V2008,
        V2012,
        V2017
    }

    public class SqlServerEntityStorage : RelationalDatabaseEntityStorageBase
    {
        private readonly SqlServerVersion _serverVersion;

        public static SqlServerEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            var storage = new SqlServerEntityStorage(options.StorageType, options.ConnectionString)
            {
                IgnoreCase = options.IgnoreCase,
                RetryTimes = options.StorageRetryTimes,
                UseTransaction = options.StorageUseTransaction
            };
            return storage;
        }

        public SqlServerEntityStorage(StorageType storageType = StorageType.InsertIgnoreDuplicate,
            string connectionString = null, SqlServerVersion version = SqlServerVersion.V2017) : base(storageType,
            connectionString)
        {
            _serverVersion = version;
        }

        protected override IDbConnection CreateDbConnection(string connectString)
        {
            return new SqlConnection(connectString);
        }

        protected override SqlStatements GenerateSqlStatements(TableMetadata tableMetadata)
        {
            var sqlStatements = new SqlStatements
            {
                InsertSql = GenerateInsertSql(tableMetadata),
                InsertIgnoreDuplicateSql = GenerateInsertSql(tableMetadata),
                InsertAndUpdateSql = GenerateInsertAndUpdateSql(tableMetadata),
                UpdateSql = GenerateUpdateSql(tableMetadata),
                CreateTableSql = GenerateCreateTableSql(tableMetadata),
                CreateDatabaseSql = GenerateCreateDatabaseSql(tableMetadata)
            };
            return sqlStatements;
        }

        private string GenerateCreateDatabaseSql(TableMetadata tableMetadata)
        {
            if (string.IsNullOrWhiteSpace(tableMetadata.Schema.Database))
            {
                return null;
            }

            var database = GetNameSql(tableMetadata.Schema.Database);
            switch (_serverVersion)
            {
                case SqlServerVersion.V2000:
                case SqlServerVersion.V2005:
                case SqlServerVersion.V2008:
                {
                    return
                        $"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{database}') CREATE DATABASE {database};";
                }
                case SqlServerVersion.V2012:
                case SqlServerVersion.V2017:
                {
                    return
                        $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{database}') CREATE DATABASE {database};";
                }
                default:
                {
                    return
                        $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{database}') CREATE DATABASE {database};";
                }
            }
        }

        /// <summary>
        /// 构造创建数据表的SQL语句
        /// </summary>
        /// <param name="tableMetadata"></param>
        /// <returns>SQL语句</returns>
        private string GenerateCreateTableSql(TableMetadata tableMetadata)
        {
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;

            var tableName = GetNameSql(tableMetadata.Schema.Table);
            var database = GetNameSql(tableMetadata.Schema.Database);

            var builder = string.IsNullOrWhiteSpace(database)
                ? new StringBuilder($"IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (")
                : new StringBuilder(
                    $"USE {database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");

            foreach (var column in tableMetadata.Columns)
            {
                var isPrimary = tableMetadata.IsPrimary(column.Key);

                var columnSql = GenerateColumnSql(column.Value, isPrimary);

                if (isAutoIncrementPrimary && isPrimary)
                {
                    builder.Append($"{columnSql} IDENTITY(1,1), ");
                }
                else
                {
                    builder.Append($"{columnSql}, ");
                }
            }

            builder.Remove(builder.Length - 2, 2);

            if (tableMetadata.HasPrimary)
            {
                var primaryKeys = string.Join(", ", tableMetadata.Primary.Select(c => $"[{GetNameSql(c)}]"));
                builder.Append(
                    $", CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({primaryKeys}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON , ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");
            }
            else
            {
                builder.Append(") ON [PRIMARY];");
            }

            if (tableMetadata.Indexes.Count > 0)
            {
                foreach (var index in tableMetadata.Indexes)
                {
                    var name = index.Name;
                    var columnNames = string.Join(", ", index.Columns.Select(c => $"[{GetNameSql(c)}]"));
                    builder.Append(
                        $"CREATE {(index.IsUnique ? "UNIQUE" : "")} NONCLUSTERED INDEX [INDEX_{name}] ON {tableName} ({columnNames}) {(StorageType == StorageType.InsertIgnoreDuplicate ? "WITH (IGNORE_DUP_KEY = ON)" : "")};");
                }
            }

            var sql = builder.ToString();
            return sql;
        }

        private string GenerateInsertSql(TableMetadata tableMetadata)
        {
            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
            // 去掉自增主键
            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();

            var columnsSql = string.Join(", ", insertColumns.Select(c => $"[{GetNameSql(c.Key)}]"));

            var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

            var tableSql = GenerateTableSql(tableMetadata);

            var sql = $"INSERT INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql});";
            return sql;
        }

        private string GenerateUpdateSql(TableMetadata tableMetadata)
        {
            if (tableMetadata.Updates == null || tableMetadata.Updates.Count == 0)
            {
                return null;
            }

            var where = "";
            foreach (var column in tableMetadata.Primary)
            {
                where += $" [{GetNameSql(column)}] = @{column} AND";
            }

            where = where.Substring(0, where.Length - 3);

            var setCols = string.Join(", ", tableMetadata.Updates.Select(c => $"[{GetNameSql(c)}] = @{c}"));

            var tableSql = GenerateTableSql(tableMetadata);
            var sql = $"UPDATE {tableSql} SET {setCols} WHERE {where};";
            return sql;
        }

        private string GenerateInsertAndUpdateSql(TableMetadata tableMetadata)
        {
            if (!tableMetadata.HasPrimary)
            {
                Logger?.LogWarning("实体没有设置主键, 无法生成 InsertAndUpdate 语句");
                return null;
            }

            // UPDATE MyTable SET FieldA=@FieldA WHERE Key=@Key IF @@ROWCOUNT = 0 INSERT INTO MyTable (FieldA) VALUES (@FieldA)
            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
            // 去掉自增主键
            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();

            var columnsSql = string.Join(", ", insertColumns.Select(c => $"[{GetNameSql(c.Key)}]"));

            var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

            var where = "";
            foreach (var column in tableMetadata.Primary)
            {
                where += $" [{GetNameSql(column)}] = @{column} AND";
            }

            where = where.Substring(0, where.Length - 3);

            var tableSql = GenerateTableSql(tableMetadata);

            var setCols = string.Join(", ", insertColumns.Select(c => $"[{GetNameSql(c.Key)}] = @{c.Key}"));
            var sql =
                $"UPDATE {tableSql} SET {setCols} WHERE {where} IF @@ROWCOUNT = 0 INSERT INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql});";
            return sql;
        }

        private string GenerateTableSql(TableMetadata tableMetadata)
        {
            var tableName = GetNameSql(tableMetadata.Schema.Table);
            var database = GetNameSql(tableMetadata.Schema.Database);
            return string.IsNullOrWhiteSpace(database) ? $"[{tableName}]" : $"[{database}].[dbo].[{tableName}]";
        }

        private string GenerateColumnSql(Column column, bool isPrimary)
        {
            var columnName = GetNameSql(column.Name);
            var dataType = GenerateDataTypeSql(column.Type, column.Length);
            if (isPrimary || column.Required)
            {
                dataType = $"{dataType} NOT NULL";
            }

            return $"[{columnName}] {dataType}";
        }

        private string GenerateDataTypeSql(string type, int length)
        {
            string dataType;

            switch (type)
            {
                case BoolType:
                {
                    dataType = "BIT";
                    break;
                }
                case DateTimeType:
                case DateTimeOffsetType:
                {
                    dataType = "DATETIME DEFAULT(GETDATE())";
                    break;
                }

                case DecimalType:
                {
                    dataType = "DECIMAL(18,2)";
                    break;
                }
                case DoubleType:
                {
                    dataType = "FLOAT";
                    break;
                }
                case FloatType:
                {
                    dataType = "FLOAT";
                    break;
                }
                case IntType:
                {
                    dataType = "INT";
                    break;
                }
                case LongType:
                {
                    dataType = "BIGINT";
                    break;
                }
                case ByteType:
                {
                    dataType = "INT";
                    break;
                }
                default:
                {
                    dataType = length <= 0 || length > 8000 ? "NVARCHAR(MAX)" : $"VARCHAR({length})";
                    break;
                }
            }

            return dataType;
        }
    }
}