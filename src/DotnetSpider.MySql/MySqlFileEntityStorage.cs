using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage.Model;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow.Storage
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
	/// 把解析到的爬虫实体数据存成 SQL 文件, 支持两种模式
	/// LoadFile 是批量导入模式通过命令 LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES; 还原。
	/// InsertSql 是完整的 Insert SQL 语句, 需要一条条执行来导入数据
	/// </summary>
	public class MySqlFileEntityStorage : EntityFileStorageBase
	{
		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase { get; set; } = true;

		public MySqlFileType MySqlFileType { get; set; }

		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public static MySqlFileEntityStorage CreateFromOptions(SpiderOptions options)
		{
			var fileType = string.IsNullOrWhiteSpace(options.MySqlFileType)
				? MySqlFileType.InsertSql
				: (MySqlFileType)Enum.Parse(typeof(MySqlFileType),
					options.MySqlFileType);
			return new MySqlFileEntityStorage(fileType)
			{
				IgnoreCase = options.StorageIgnoreCase
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

		protected override Task<DataFlowResult> Store(DataFlowContext context)
		{
			foreach (var item in context.GetParseData())
			{
				var tableMetadata = (TableMetadata)context[item.Key];
				switch (MySqlFileType)
				{
					case MySqlFileType.LoadFile:
						{
							WriteLoadFile(context, tableMetadata, item.Value);
							break;
						}
					case MySqlFileType.InsertSql:
						{
							WriteInsertFile(context, tableMetadata, item.Value);
							break;
						}
				}
			}

			return Task.FromResult(DataFlowResult.Success);
		}

		private void WriteInsertFile(DataFlowContext context, TableMetadata tableMetadata, IParseResult items)
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

			var writer = CreateOrOpen(context, tableMetadata, "mysql");
			lock (writer)
			{
				writer.WriteLine(builder.ToString());
			}

			builder.Clear();
		}

		private void WriteLoadFile(DataFlowContext context, TableMetadata tableMetadata, IParseResult items)
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

			var writer = CreateOrOpen(context, tableMetadata, "mysql");
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
	}
}
