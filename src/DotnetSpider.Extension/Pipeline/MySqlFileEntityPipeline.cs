using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public sealed class MySqlFileEntityPipeline : BaseEntityDbPipeline
	{
		private readonly object _locker = new object();

		public enum FileType
		{
			LoadFile,
			InsertSql
		}

		private FileType Type { get; }

		private string DataFolder { get; set; }

		public MySqlFileEntityPipeline(FileType fileType = FileType.LoadFile)
		{
			Type = fileType;
		}

		public override void InitPipeline(ISpider spider)
		{
			if (string.IsNullOrEmpty(DataFolder))
			{
				DataFolder = Path.Combine(Env.BaseDirectory, spider.Identity, "mysql");
			}
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}
			lock (_locker)
			{
				if (EntityAdapters.TryGetValue(entityName, out var metadata))
				{
					switch (Type)
					{
						case FileType.LoadFile:
							{
								SaveLoadFile(metadata, datas);
								break;
							}
						case FileType.InsertSql:
							{
								SaveInsertSqlFile(metadata, datas);
								break;
							}
					}
				}
				return datas.Count;
			}
		}

		private void SaveInsertSqlFile<T>(EntityAdapter metadata, List<T> items)
		{
			var tableName = metadata.Table.CalculateTableName();
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.Table.Database}.{tableName}.sql"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				//{Environment.NewLine}
				builder.Append($"INSERT IGNORE INTO `{metadata.Table.Database}`.`{tableName}` (");
				var lastColumn = metadata.Columns.Last();
				foreach (var column in metadata.Columns)
				{
					builder.Append(column == lastColumn ? $"`{column.Name}`" : $"`{column.Name}`, ");
				}
				builder.Append(") VALUES (");

				foreach (var column in metadata.Columns)
				{
					var token = column.Property.GetValue(entry);
					var value = token == null ? "" : MySqlHelper.EscapeString(token.ToString());

					builder.Append(column == lastColumn ? $"'{value}'" : $"'{value}', ");
				}
				builder.Append($");{System.Environment.NewLine}");
			}
			File.AppendAllText(fileInfo.FullName, builder.ToString());
		}

		private void SaveLoadFile<T>(EntityAdapter metadata, List<T> items)
		{
			var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.Table.Database}.{metadata.Table.Name}.data"));
			StringBuilder builder = new StringBuilder();
			foreach (var entry in items)
			{
				builder.Append("@END@");
				foreach (var column in metadata.Columns)
				{
					var value = column.Property.GetValue(entry);
					if (value != null)
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

		protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
		{
			return null;
		}

		protected override void InitAllSqlOfEntity(EntityAdapter adapter)
		{
		}

		internal override void InitDatabaseAndTable()
		{
		}
	}
}
