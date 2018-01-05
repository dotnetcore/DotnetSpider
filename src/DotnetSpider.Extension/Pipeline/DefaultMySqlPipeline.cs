using DotnetSpider.Core.Pipeline;
using System.Collections.Generic;
using DotnetSpider.Core;
using System.Configuration;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Pipeline
{
	public class DefaultMySqlPipeline : BasePipeline
	{
		public ConnectionStringSettings ConnectionStringSettings { get; private set; }

		public string Database { get; }

		public string TableName { get; }

		public DefaultMySqlPipeline(string database = "test", string tableName = "myHtml") : this(null, database, tableName)
		{
		}

		public DefaultMySqlPipeline(string connectString, string database, string tableName)
		{
			if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(tableName))
			{
				throw new SpiderException("Database or table name should not be null or empty.");
			}
			InitConnectStringSettings(connectString);
			Database = database;
			TableName = tableName;
			InitDatabaseAndTable(Database, TableName);
		}

		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			var results = new List<DefaulHtmlContent>();
			foreach (var resultItem in resultItems)
			{
				results.Add(new DefaulHtmlContent
				{
					Url = resultItem.GetResultItem("url")?.ToString(),
					Title = resultItem.GetResultItem("title")?.ToString(),
					Html = resultItem.GetResultItem("html")?.ToString()
				});
			}
			using (var conn = ConnectionStringSettings.CreateDbConnection())
			{
				conn.MyExecute($"INSERT IGNORE `{Database}`.`{TableName}` (`url`, `title`, `html`) VALUES (@Url, @Title, @Html);", results);

			}
		}

		private void InitConnectStringSettings(string connectString)
		{
			ConnectionStringSettings connectionStringSettings;
			if (!string.IsNullOrEmpty(connectString))
			{
				connectionStringSettings = new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
			}
			else
			{
				if (Env.DataConnectionStringSettings != null)
				{
					connectionStringSettings = Env.DataConnectionStringSettings;
				}
				else
				{
					throw new SpiderException("DataConnection is unfound in app.config.");
				}
			}
			ConnectionStringSettings = connectionStringSettings;
		}

		private void InitDatabaseAndTable(string database, string tableName)
		{
			using (var conn = ConnectionStringSettings.CreateDbConnection())
			{
				conn.MyExecute($"CREATE SCHEMA IF NOT EXISTS `{database}` DEFAULT CHARACTER SET utf8mb4 ;");
				conn.MyExecute($"CREATE TABLE IF NOT EXISTS `{database}`.`{tableName}` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `url` varchar(300) DEFAULT NULL, `title` varchar(300) DEFAULT NULL, `html` text, `cdate` timestamp NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (`id`), KEY `url_index` (`url`) USING BTREE) DEFAULT CHARSET=utf8mb4;");
			}
		}

		public class DefaulHtmlContent
		{
			public string Url { get; set; }
			public string Title { get; set; }
			public string Html { get; set; }
		}
	}
}
