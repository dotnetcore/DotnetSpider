using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class MySqlFileEntityPipeline : BaseEntityPipeline
	{
		private readonly object _locker = new object();

		public enum FileType
		{
			LoadFile,
			InsertSql
		}

		public FileType Type { get; set; }

		public string DataFolder { get; set; }

		public MySqlFileEntityPipeline(FileType fileType = FileType.LoadFile)
		{
			Type = fileType;
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			if (string.IsNullOrEmpty(DataFolder))
			{
				DataFolder = Path.Combine(Environment.BaseDirectory, spider.Identity, "mysql");
			}
		}

		public override void Process(string entityName, List<DataObject> items)
		{
			if (items == null || items.Count == 0)
			{
				return;
			}

			lock (_locker)
			{
				if (Entities.TryGetValue(entityName, out var metadata))
				{
					switch (Type)
					{
						case FileType.LoadFile:
							{
								SaveLoadFile(metadata, items);
								break;
							}
						case FileType.InsertSql:
							{
								SaveInsertSqlFile(metadata, items);
								break;
							}
					}
				}
			}
		}

		private void SaveInsertSqlFile(EntityDefine metadata, List<DataObject> items)
		{
			var tableName = metadata.TableInfo.CalculateTableName();
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.TableInfo.Database}.{tableName}.sql"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				//{Environment.NewLine}
				builder.Append($"INSERT IGNORE INTO `{metadata.TableInfo.Database}`.`{tableName}` (");
				var lastColumn = metadata.Columns.Last();
				foreach (var column in metadata.Columns)
				{
					builder.Append(column == lastColumn ? $"`{column.Name}`" : $"`{column.Name}`, ");
				}
				builder.Append(") VALUES (");

				foreach (var column in metadata.Columns)
				{
					var token = entry[$"$.{column.Name}"];
					var value = token == null ? "" : MySqlHelper.EscapeString(token.ToString());

					builder.Append(column == lastColumn ? $"'{value}'" : $"'{value}', ");
				}
				builder.Append($");{System.Environment.NewLine}");
			}
			File.AppendAllText(fileInfo.FullName, builder.ToString());
		}

		private void SaveLoadFile(EntityDefine metadata, List<DataObject> items)
		{
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.TableInfo.Database}.{metadata.TableInfo.Name}.data"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				builder.Append("@END@");
				foreach (var column in metadata.Columns)
				{
					var value = entry[$"$.{column.Name}"]?.ToString();
					if (!string.IsNullOrEmpty(value))
					{
						builder.Append("#").Append(value).Append("#").Append("$");
					}
					else
					{
						builder.Append("##$");
					}
				}
			}
			File.AppendAllText(fileInfo.FullName, builder.ToString());
		}
	}
}
