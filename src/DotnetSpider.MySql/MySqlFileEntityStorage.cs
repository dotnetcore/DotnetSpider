using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace DotnetSpider.MySql
{
	public class MySqlFileOptions
	{
		private readonly IConfiguration _configuration;

		public MySqlFileOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public MySqlFileType Type => string.IsNullOrWhiteSpace(_configuration["MySqlFile:Type"])
			? MySqlFileType.LoadFile
			: (MySqlFileType)Enum.Parse(typeof(MySqlFileType), _configuration["MySqlFile:Type"]);

		public bool IgnoreCase => string.IsNullOrWhiteSpace(_configuration["MySqlFile:IgnoreCase"]) ||
		                          bool.Parse(_configuration["MySqlFile:IgnoreCase"]);
	}

	/// <summary>
	/// 把解析到的爬虫实体数据存成 SQL 文件, 支持两种模式
	/// LoadFile 是批量导入模式通过命令 LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES; 还原。
	/// InsertSql 是完整的 Insert SQL 语句, 需要一条条执行来导入数据
	/// </summary>
	public class MySqlFileEntityStorage : EntityFileStorageBase
	{
		private readonly ConcurrentDictionary<string, StreamWriter> _streamWriters =
			new();

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase { get; set; }

		public MySqlFileType MySqlFileType { get; set; }

		public override Task InitializeAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="configuration">配置</param>
		/// <returns></returns>
		public static MySqlFileEntityStorage CreateFromOptions(IConfiguration configuration)
		{
			var options = new MySqlFileOptions(configuration);
			return new MySqlFileEntityStorage(options.Type, options.IgnoreCase);
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="fileType">文件类型</param>
		/// <param name="ignoreCase"></param>
		public MySqlFileEntityStorage(MySqlFileType fileType = MySqlFileType.LoadFile, bool ignoreCase = false)
		{
			MySqlFileType = fileType;
			IgnoreCase = ignoreCase;
		}

		protected override async Task HandleAsync(DataFlowContext context, TableMetadata tableMetadata, IEnumerable entities)
		{
			var writer = _streamWriters.GetOrAdd(tableMetadata.TypeName,
				_ => OpenWrite(context, tableMetadata, "sql"));

			switch (MySqlFileType)
			{
				case MySqlFileType.LoadFile:
				{
					await WriteLoadFileAsync(writer, tableMetadata, entities);
					break;
				}
				case MySqlFileType.InsertSql:
				{
					await WriteInsertFile(writer, tableMetadata, entities);
					break;
				}
			}
		}

		public override void Dispose()
		{
			foreach (var streamWriter in _streamWriters)
			{
				streamWriter.Value.Dispose();
			}

			base.Dispose();
		}

		private async Task WriteLoadFileAsync(StreamWriter writer, TableMetadata tableMetadata, IEnumerable items)
		{
			var builder = new StringBuilder();
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

			await writer.WriteLineAsync(builder.ToString());
		}

		private async Task WriteInsertFile(StreamWriter writer, TableMetadata tableMetadata, IEnumerable items)
		{
			var builder = new StringBuilder();
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

			await writer.WriteLineAsync(builder.ToString());

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
	}
}
