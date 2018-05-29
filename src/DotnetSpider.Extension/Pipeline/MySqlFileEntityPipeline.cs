﻿using System.Collections.Generic;
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
	public  class MySqlFileEntityPipeline : BaseEntityDbPipeline
	{
		//private readonly object _locker = new object();
        private readonly Dictionary<string, StreamWriter> _writers = new Dictionary<string, StreamWriter>();
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

		public override int Process(string entityName, IList<dynamic> datas,ISpider spider)
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
                switch (Type)
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
