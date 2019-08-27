using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow.Storage
{
	public class MySqlRequestIndexStorage : DataFlowBase
	{
		private readonly string _connectionString;

		private readonly ConcurrentDictionary<string, object> _executedCache =
			new ConcurrentDictionary<string, object>();

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectionString">连接字符串</param>
		public MySqlRequestIndexStorage(string connectionString)
		{
			_connectionString = connectionString;
		}

		public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
		{
			var tableName = $"dotnetspider.spider_request_index_{DateTimeHelper.Monday:yyyy_MM_dd}";

			var sql = $@"
 create table if not exists {tableName}
(
    spider_id     varchar(48)                         not null,
    hash          varchar(48)                         not null,
    creation_time timestamp default CURRENT_TIMESTAMP null,
    primary key (spider_id, hash)
);";
			if (_executedCache.TryAdd(sql, new object()))
			{
				using (var conn = new MySqlConnection(_connectionString))
				{
					await conn.ExecuteAsync(sql);
				}
			}

			sql = $"INSERT INTO {tableName} (`spider_id`, `hash`) VALUES (@spider_id, @hash);";
			var item = new
			{
				spider_id = context.Response.Request.OwnerId,
				// 因为 request 中包含 ownerId, 因此即便是完美一样的请求, 对于不同批次的爬虫来说计算出来的 hash 也是不相同的, 得于在 HBase 中保存数据
				hash = context.Response.Request.Hash
			};

			for (int i = 0; i < 10; ++i)
			{
				try
				{
					using (var conn = new MySqlConnection(_connectionString))
					{
						await conn.ExecuteAsync(sql, item);
					}

					return DataFlowResult.Success;
				}
				catch (Exception ex)
				{
					Logger.LogError($"Storage request {context.Response.Request.Url} index failed [{i + 1}]: {ex}");
				}
			}

			return DataFlowResult.Failed;
		}

		public override async Task InitAsync()
		{
			using (var conn = new MySqlConnection(_connectionString))
			{
				await conn.ExecuteAsync("CREATE SCHEMA IF NOT EXISTS dotnetspider DEFAULT CHARACTER SET utf8mb4;");
			}

			await base.InitAsync();
		}
	}
}
