using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage.Model;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Data.Storage
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public enum MySqlFileType
    {
        /// <summary>
        /// LOAD
        /// </summary>
        LoadFile,

        /// <summary>
        /// INSERT SQL语句
        /// </summary>
        InsertSql
    }

    /// <summary>
    /// 把解析到的爬虫实体数据存成SQL文件, 支持两种模式
    /// LoadFile是批量导入模式通过命令 LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES; 还原。
    /// InsertSql是完整的Insert SQL语句, 需要一条条执行来导入数据
    /// </summary>
    public class MySqlFileEntityStorage : EntityStorageBase
    {
        private readonly ConcurrentDictionary<string, StreamWriter> _writers =
            new ConcurrentDictionary<string, StreamWriter>();

        /// <summary>
        /// 数据库忽略大小写
        /// </summary>
        public bool IgnoreCase { get; set; } = true;

        public MySqlFileType MySqlFileType { get; set; }

        public static MySqlFileEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            var fileType = string.IsNullOrWhiteSpace(options.MySqlFileType)
                ? MySqlFileType.InsertSql
                : (MySqlFileType) Enum.Parse(typeof(MySqlFileType),
                    options.MySqlFileType);
            return new MySqlFileEntityStorage(fileType)
            {
                IgnoreCase = options.IgnoreCase
            };
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="fileType">文件类型</param>
        public MySqlFileEntityStorage(MySqlFileType fileType = MySqlFileType.LoadFile)
        {
            MySqlFileType = fileType;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var writer in _writers)
            {
                try
                {
                    writer.Value.Dispose();
                }
                catch (Exception e)
                {
                    Logger?.LogError($"释放 MySqlFile 文件 {writer.Key} 失败: {e}");
                }
            }
        }

        protected override Task<DataFlowResult> Store(DataFlowContext context)
        {
            foreach (var item in context.GetParseItems())
            {
                var tableMetadata = (TableMetadata) context[item.Key];
                var file = Path.Combine(Framework.BaseDirectory, "mysql-files",
                    $"{GenerateFileName(tableMetadata)}.sql");

                switch (MySqlFileType)
                {
                    case MySqlFileType.LoadFile:
                    {
                        WriteLoadFile(file, tableMetadata, item.Value);
                        break;
                    }
                    case MySqlFileType.InsertSql:
                    {
                        WriteInsertFile(file, tableMetadata, item.Value);
                        break;
                    }
                }
            }

            return Task.FromResult(DataFlowResult.Success);
        }

        private void WriteInsertFile(string file, TableMetadata tableMetadata, IParseResult items)
        {
            StringBuilder builder = new StringBuilder();
            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;
            var tableSql = GenerateTableSql(tableMetadata);

            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();
            foreach (var item in items)
            {
                builder.Append($"INSERT IGNORE INTO {tableSql} (");
                var lastColumn = insertColumns.Last();
                foreach (var column in insertColumns)
                {
                    builder.Append(column.Equals(lastColumn) ? $"`{column.Key}`" : $"`{column.Key}`, ");
                }

                builder.Append(") VALUES (");

                foreach (var column in insertColumns)
                {
                    var value = column.Value.PropertyInfo.GetValue(item);
                    value = value == null ? "" : MySqlHelper.EscapeString(value.ToString());
                    builder.Append(column.Equals(lastColumn) ? $"'{value}'" : $"'{value}', ");
                }

                builder.Append($");{Environment.NewLine}");
            }

            StreamWriter writer = CreateOrOpen(file);
            lock (writer)
            {
                writer.WriteLine(builder.ToString());
            }

            builder.Clear();
        }

        private void WriteLoadFile(string file, TableMetadata tableMetadata, IParseResult items)
        {
            StringBuilder builder = new StringBuilder();
            var columns = tableMetadata.Columns;
            var isAutoIncrementPrimary = tableMetadata.IsAutoIncrementPrimary;

            var insertColumns =
                (isAutoIncrementPrimary ? columns.Where(c1 => c1.Key != tableMetadata.Primary.First()) : columns)
                .ToArray();
            foreach (var item in items)
            {
                builder.Append("@END@");
                foreach (var column in insertColumns)
                {
                    var value = column.Value.PropertyInfo.GetValue(item);
                    value = value == null ? "" : MySqlHelper.EscapeString(value.ToString());
                    builder.Append("#").Append(value).Append("#").Append("$");
                }
            }

            StreamWriter writer = CreateOrOpen(file);
            lock (writer)
            {
                writer.WriteLine(builder.ToString());
            }

            builder.Clear();
        }

        protected virtual string GenerateTableSql(TableMetadata tableMetadata)
        {
            var tableName = GetNameSql(tableMetadata.Schema.Table);
            var database = GetNameSql(tableMetadata.Schema.Database);
            return string.IsNullOrWhiteSpace(database) ? $"`{tableName}`" : $"`{database}`.`{tableName}`";
        }

        private string GetNameSql(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return IgnoreCase ? name.ToLowerInvariant() : name;
        }

        private string GenerateFileName(TableMetadata tableMetadata)
        {
            return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
                ? $"{tableMetadata.Schema.Table}"
                : $"{tableMetadata.Schema.Database}.{tableMetadata.Schema.Table}";
        }

        private StreamWriter CreateOrOpen(string file)
        {
            return _writers.GetOrAdd(file, x =>
            {
                var folder = Path.GetDirectoryName(x);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return new StreamWriter(File.OpenWrite(x), Encoding.UTF8);
            });
        }
    }
}