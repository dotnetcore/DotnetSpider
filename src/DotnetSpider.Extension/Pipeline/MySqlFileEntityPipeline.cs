using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Extension.Model;
using System;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class MySqlFileEntityPipeline : BaseEntityPipeline
	{
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
				DataFolder = Path.Combine(Core.Infrastructure.Environment.BaseDirectory, spider.Identity, "mysql");
			}
		}

		public override void Process(string entityName, List<JObject> items)
		{
			if (items == null || items.Count == 0)
			{
				return;
			}

			lock (this)
			{
				Entity metadata;
				if (Entities.TryGetValue(entityName, out metadata))
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

		private void SaveInsertSqlFile(Entity metadata, List<JObject> items)
		{
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.Table.Database}.{metadata.Table.Name}.sql"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				//{Environment.NewLine}
				builder.Append($"INSERT IGNORE INTO `{metadata.Table.Database}`.`{metadata.Table.Name}` (");
				var lastColumn = metadata.Fields.Last();
				foreach (var column in metadata.Fields)
				{
					builder.Append(column == lastColumn ? $"`{column.Name}`" : $"`{column.Name}`, ");
				}
				builder.Append(") VALUES (");

				foreach (var column in metadata.Fields)
				{
					var token = entry.SelectToken($"$.{column.Name}");
					var value = token == null ? "" : MySqlHelper.EscapeString(token.Value<string>());

					builder.Append(column == lastColumn ? $"'{value}'" : $"'{value}', ");
				}
				builder.Append($");{Environment.NewLine}");
			}
			File.AppendAllText(fileInfo.FullName, builder.ToString());
		}

		private void SaveLoadFile(Entity metadata, List<JObject> items)
		{
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.Table.Database}.{metadata.Table.Name}.data"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				builder.Append("@END@");
				foreach (var column in metadata.Fields)
				{
					var value = entry.SelectToken($"$.{column.Name}")?.ToString();
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
