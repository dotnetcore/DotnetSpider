using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存成SQL文件, 支持两种模式
	/// LoadFile是批量导入模式通过命令 LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES; 还原。
	/// InsertSql是完整的Insert SQL语句, 需要一条条执行来导入数据
	/// </summary>
	public class MySqlEntityFilePipeline : ModelPipeline
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
		public MySqlEntityFilePipeline(FileType fileType = FileType.LoadFile)
		{
			_type = fileType;
		}


		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="model">数据模型</param>
		/// <param name="datas">数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}

			StreamWriter writer;
			var tableName = model.Table.FullName;
			var identity = GetIdentity(sender);
			var dataFolder = Path.Combine(Env.BaseDirectory, "mysql", identity);
			var mysqlFile = Path.Combine(dataFolder, $"{model.Table.Database}.{tableName}.sql");
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
						AppendLoadFile(writer, model, datas);
						break;
					}
				case FileType.InsertSql:
					{
						AppendInsertSqlFile(writer, model, datas);
						break;
					}
			}

			return datas.Count;
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

		private void AppendInsertSqlFile(StreamWriter writer, IModel model, IEnumerable<dynamic> items)
		{
			StringBuilder builder = new StringBuilder();
			foreach (var item in items)
			{
				//{Environment.NewLine}
				builder.Append($"INSERT IGNORE INTO `{model.Table.Database}`.`{model.Table.FullName}` (");
				var lastColumn = model.Fields.Last();
				foreach (var column in model.Fields)
				{
					builder.Append(column.Equals(lastColumn) ? $"`{column.Name}`" : $"`{column.Name}`, ");
				}

				builder.Append(") VALUES (");

				foreach (var column in model.Fields)
				{
					var value = item[column.Name];
					value = value == null ? "" : MySqlHelper.EscapeString(value.ToString());
					builder.Append(column.Equals(lastColumn) ? $"'{value}'" : $"'{value}', ");
				}

				builder.Append($");{System.Environment.NewLine}");
			}

			writer.Write(builder.ToString());
		}

		private void AppendLoadFile(StreamWriter writer, IModel model, IEnumerable<dynamic> items)
		{
			StringBuilder builder = new StringBuilder();
			foreach (var item in items)
			{
				builder.Append("@END@");
				foreach (var column in model.Fields)
				{
					var value = item[column.Name];
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