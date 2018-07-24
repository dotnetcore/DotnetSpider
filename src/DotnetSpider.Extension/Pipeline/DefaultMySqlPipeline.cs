using DotnetSpider.Core.Pipeline;
using System.Collections.Generic;
using DotnetSpider.Core;
using System.Configuration;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 默认的MySql数据管道, 用于存储下载的HTML数据: 按URL, 标题, 内容 存储
	/// </summary>
	public class DefaultMySqlPipeline : BasePipeline
	{
		/// <summary>
		/// 数据库连接配置
		/// </summary>
		public ConnectionStringSettings ConnectionStringSettings { get; private set; }

		/// <summary>
		/// 数据库名称
		/// </summary>
		public string Database { get; }

		/// <summary>
		/// 数据表名
		/// </summary>
		public string TableName { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="database">数据库名</param>
		/// <param name="tableName">表名</param>
		public DefaultMySqlPipeline(string database = "test", string tableName = "myHtml") : this(null, database, tableName)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		/// <param name="database">数据库名</param>
		/// <param name="tableName">表名</param>
		public DefaultMySqlPipeline(string connectString, string database, string tableName)
		{
			if (string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(tableName))
			{
				throw new SpiderException("Database or table name should not be null or empty");
			}
			InitConnectStringSettings(connectString);
			Database = database;
			TableName = tableName;
			InitDatabaseAndTable(Database, TableName);
		}

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			var results = new List<dynamic>();
			foreach (var resultItem in resultItems)
			{
				results.Add(new
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
			if (!string.IsNullOrWhiteSpace(connectString))
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
					throw new SpiderException("DataConnection is unfound in app.config");
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
	}
}
