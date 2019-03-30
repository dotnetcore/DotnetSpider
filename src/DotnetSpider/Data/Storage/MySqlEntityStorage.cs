using System.Data;
using System.Linq;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Data.Storage.Model;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Data.Storage
{
    public class MySqlEntityStorage : RelationalDatabaseEntityStorageBase
    {
        public static MySqlEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            var storage = new MySqlEntityStorage(options.StorageType, options.ConnectionString)
            {
                IgnoreCase = options.IgnoreCase,
                RetryTimes = options.StorageRetryTimes,
                UseTransaction = options.StorageUseTransaction
            };
            return storage;
        }

        public MySqlEntityStorage(StorageType storageType = StorageType.InsertIgnoreDuplicate,
            string connectionString = null) : base(storageType,
            connectionString)
        {
        }

        protected override IDbConnection CreateDbConnection(string connectString)
        {
            return new MySqlConnection(connectString);
        }

        protected override SqlStatements GenerateSqlStatements(TableMetadata tableMetadata)
        {
            var sqlStatements = new SqlStatements
            {
                InsertSql = GenerateInsertSql(tableMetadata, false),
                InsertIgnoreDuplicateSql = GenerateInsertSql(tableMetadata, true),
                InsertAndUpdateSql = GenerateInsertAndUpdateSql(tableMetadata),
                UpdateSql = GenerateUpdateSql(tableMetadata),
                CreateTableSql = GenerateCreateTableSql(tableMetadata),
                CreateDatabaseSql = GenerateCreateDatabaseSql(tableMetadata)
            };
            return sqlStatements;
        }

        protected virtual string GenerateCreateDatabaseSql(TableMetadata tableMetadata)
        {
            return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
                ? ""
                : $"CREATE SCHEMA IF NOT EXISTS `{GetNameSql(tableMetadata.Schema.Database)}` DEFAULT CHARACTER SET utf8mb4;";
        }

        /// <summary>
        /// 构造创建数据表的SQL语句
        /// </summary>
        /// <param name="tableMetadata"></param>
        /// <returns>SQL语句</returns>
        protected virtual string GenerateCreateTableSql(TableMetadata tableMetadata)
        {
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;

            var tableSql = GenerateTableSql(tableMetadata);

            var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableSql} (");

            foreach (var column in tableMetadata.Columns)
            {
                var isPrimary = tableMetadata.IsPrimary(column.Key);

                var columnSql = GenerateColumnSql(column.Value, isPrimary);

                if (isAutoIncrementPrimary && isPrimary)
                {
                    builder.Append($"{columnSql} AUTO_INCREMENT, ");
                }
                else
                {
                    builder.Append($"{columnSql}, ");
                }
            }

            builder.Remove(builder.Length - 2, 2);

            if (tableMetadata.Primary != null && tableMetadata.Primary.Count > 0)
            {
                builder.Append(
                    $", PRIMARY KEY ({string.Join(", ", tableMetadata.Primary.Select(c => $"`{GetNameSql(c)}`"))})");
            }

            if (tableMetadata.Indexes.Count > 0)
            {
                foreach (var index in tableMetadata.Indexes)
                {
                    var name = index.Name;
                    var columnNames = string.Join(", ", index.Columns.Select(c => $"`{GetNameSql(c)}`"));
                    builder.Append($", {(index.IsUnique ? "UNIQUE" : "")} KEY `{name}` ({columnNames})");
                }
            }

            builder.Append(")");
            var sql = builder.ToString();
            return sql;
        }

        protected virtual string GenerateInsertSql(TableMetadata tableMetadata, bool ignoreDuplicate)
        {
            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
            // 去掉自增主键
            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();

            var columnsSql = string.Join(", ", insertColumns.Select(c => $"`{GetNameSql(c.Key)}`"));

            var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

            var tableSql = GenerateTableSql(tableMetadata);

            var sql =
                $"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql});";
            return sql;
        }

        protected virtual string GenerateUpdateSql(TableMetadata tableMetadata)
        {
            if (tableMetadata.Updates == null || tableMetadata.Updates.Count == 0)
            {
                Logger?.LogWarning("实体没有设置主键, 无法生成 Update 语句");
                return null;
            }

            var where = "";
            foreach (var column in tableMetadata.Primary)
            {
                where += $" `{GetNameSql(column)}` = @{column} AND";
            }

            where = where.Substring(0, where.Length - 3);

            var setCols = string.Join(", ", tableMetadata.Updates.Select(c => $"`{GetNameSql(c)}`= @{c}"));
            var tableSql = GenerateTableSql(tableMetadata);
            var sql = $"UPDATE {tableSql} SET {setCols} WHERE {where};";
            return sql;
        }

        protected virtual string GenerateInsertAndUpdateSql(TableMetadata tableMetadata)
        {
            if (!tableMetadata.HasPrimary)
            {
                Logger?.LogWarning("实体没有设置主键, 无法生成 InsertAndUpdate 语句");
                return null;
            }

            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
            // 去掉自增主键
            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();

            var columnsSql = string.Join(", ", insertColumns.Select(c => $"`{GetNameSql(c.Key)}`"));

            var columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Key}"));

            var tableSql = GenerateTableSql(tableMetadata);
            var setCols = string.Join(", ", insertColumns.Select(c => $"`{GetNameSql(c.Key)}`= @{c.Key}"));
            var sql =
                $"INSERT INTO {tableSql} ({columnsSql}) VALUES ({columnsParamsSql}) ON DUPLICATE key UPDATE {setCols};";
            return sql;
        }

        protected virtual string GenerateTableSql(TableMetadata tableMetadata)
        {
            var tableName = GetNameSql(tableMetadata.Schema.Table);
            var database = GetNameSql(tableMetadata.Schema.Database);
            return string.IsNullOrWhiteSpace(database) ? $"`{tableName}`" : $"`{database}`.`{tableName}`";
        }

        protected virtual string GenerateColumnSql(Column column, bool isPrimary)
        {
            var columnName = GetNameSql(column.Name);
            var dataType = GenerateDataTypeSql(column.Type, column.Length);
            if (isPrimary || column.Required)
            {
                dataType = $"{dataType} NOT NULL";
            }

            return $"`{columnName}` {dataType}";
        }

        protected virtual string GenerateDataTypeSql(string type, int length)
        {
            string dataType;

            switch (type)
            {
                case BoolType:
                {
                    dataType = "BOOL";
                    break;
                }
                case DateTimeType:
                case DateTimeOffsetType:
                {
                    dataType = "TIMESTAMP DEFAULT CURRENT_TIMESTAMP";
                    break;
                }

                case DecimalType:
                {
                    dataType = "DECIMAL(18,2)";
                    break;
                }
                case DoubleType:
                {
                    dataType = "DOUBLE";
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
                    dataType = length <= 0 || length > 8000 ? "LONGTEXT" : $"VARCHAR({length})";
                    break;
                }
            }

            return dataType;
        }
    }
}