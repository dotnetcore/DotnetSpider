using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存成SQL文件, 支持两种模式
	/// LoadFile是批量导入模式通过命令 LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES; 还原。
	/// InsertSql是完整的Insert SQL语句, 需要一条条执行来导入数据
	/// </summary>
	public class MySqlFileEntityPipeline : BaseEntityPipeline
	{
		private readonly Dictionary<string, StreamWriter> _writers = new Dictionary<string, StreamWriter>();

		/// <summary>
		/// 文件类型
		/// </summary>
		public enum FileType
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

		private readonly FileType _type;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="fileType">文件类型</param>
		public MySqlFileEntityPipeline(FileType fileType = FileType.LoadFile)
		{
			_type = fileType;
		}

		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="entityName">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				StreamWriter writer;
				var tableName = metadata.Table.CalculateTableName();
				var dataFolder = Path.Combine(Env.BaseDirectory, "mysql", spider.Identity);
				var mysqlFile = Path.Combine(dataFolder, $"{metadata.Table.Database}.{tableName}.sql");
				if (_writers.ContainsKey(mysqlFile))
				{
					writer = _writers[mysqlFile];
				}
				else
				{
					if (!Directory.Exists(dataFolder))
					{
						Directory.CreateDirectory(dataFolder);
					}
					writer = new StreamWriter(File.OpenWrite(mysqlFile), Encoding.UTF8);
					_writers.Add(mysqlFile, writer);
				}
				switch (_type)
				{
					case FileType.LoadFile:
						{
							SaveLoadFile(writer, metadata, datas);
							break;
						}
					case FileType.InsertSql:
						{
							SaveInsertSqlFile(writer, tableName, metadata, datas);
							break;
						}
				}
			}
			return datas.Count();

		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			foreach (var writer in _writers)
			{
				writer.Value.Dispose();
			}
		}

		private void SaveInsertSqlFile<T>(StreamWriter writer, string tableName, EntityAdapter metadata, IEnumerable<T> items)
		{
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
			writer.Write(builder.ToString());
		}

		private void SaveLoadFile<T>(StreamWriter writer, EntityAdapter metadata, IEnumerable<T> items)
		{
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
			writer.Write(builder.ToString());
		}
	}
}
