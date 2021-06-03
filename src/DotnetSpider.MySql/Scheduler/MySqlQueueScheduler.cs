using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace DotnetSpider.MySql.Scheduler
{
	/// <summary>
	/// {spiderId}_hash 表存着所有请求的 hash
	/// {spiderId}_set 表存着所有的请求，以 hash 做主键
	/// Enqueue 操作：
	/// 先尝试插入 hash 表，若插入成功，则再把请求插入 set 表，若是插入失败，则跳过此请求
	/// Dequeue 操作：
	/// 1. 获取分布式锁
	/// 2. 查询出需要的请求
	/// 3. 删除请求
	/// </summary>
	public abstract class MySqlQueueScheduler : IScheduler
	{
		private readonly MySqlSchedulerOptions _options;
		private readonly IRequestHasher _requestHasher;
		private string _spiderId;
		private string _insertSetSql;
		private string _totalSql;
		private string _resetDuplicateCheckSql;
		private string _successSql;
		private string _failSql;
		private string _insertQueueSql;

		protected abstract string DequeueSql { get; }

		public MySqlQueueScheduler(
			IRequestHasher requestHasher, IOptions<MySqlSchedulerOptions> options)
		{
			_options = options.Value;
			_requestHasher = requestHasher;
		}

		public async Task InitializeAsync(string spiderId)
		{
			spiderId.NotNullOrWhiteSpace(nameof(spiderId));
			_spiderId = spiderId;

			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS `{spiderId}_set`
(
    hash          varchar(32) not null primary key,
    request mediumblob not null,
    state   char(1),
    creation_time timestamp default CURRENT_TIMESTAMP not null
);");
			await conn.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS `{spiderId}_queue`
(
	id       int auto_increment primary key,
    hash          varchar(32) not null,
    request mediumblob not null,
	constraint {spiderId}_queue_hash_uindex unique (hash)
);");
			_totalSql = $"SELECT COUNT(*) FROM `{_spiderId}_set`";
			_resetDuplicateCheckSql = $"TRUNCATE `{_spiderId}_set`; TRUNCATE {_spiderId}_queue;";
			_insertSetSql = $"INSERT IGNORE INTO `{_spiderId}_set` (hash, request) VALUES (@Hash, @Request); ";
			_insertQueueSql = $"INSERT IGNORE INTO `{_spiderId}_queue` (hash, request) VALUES (@Hash, @Request);";
			_successSql = $"UPDATE `{_spiderId}_set` SET state = 'S' WHERE hash = @Hash;";
			_failSql = $"UPDATE `{_spiderId}_set` SET state = 'E' WHERE hash = @Hash;";
		}

		public async Task<IEnumerable<Request>> DequeueAsync(int count = 1)
		{
			if (count < 1)
			{
				throw new ArgumentException($"{nameof(count)} should be large than 0");
			}

			MySqlTransaction transaction = null;
			try
			{
				await using var conn = new MySqlConnection(_options.ConnectionString);
				if (conn.State != ConnectionState.Open)
				{
					await conn.OpenAsync();
				}

				transaction = await conn.BeginTransactionAsync();

				var rows = (await conn.QueryAsync<dynamic>(
						string.Format(DequeueSql, _spiderId, count),
						null, transaction))
					.Select(x => (IDictionary<string, dynamic>)x).ToList();
				if (rows.Count == 0)
				{
					await transaction.DisposeAsync();
					return Enumerable.Empty<Request>();
				}

				var ids = string.Join(",", rows.Select(x => $"'{x["id"]}'"));
				var hashes = string.Join(",", rows.Select(x => $"'{x["hash"]}'"));
				await conn.ExecuteAsync(
					$"DELETE FROM `{_spiderId}_queue` WHERE id IN ({ids}); UPDATE `{_spiderId}_set` SET state = 'P' WHERE hash IN ({hashes});",
					null, transaction);
				await transaction.CommitAsync();
				var list = new List<Request>();
				foreach (var row in rows)
				{
					var request = await MessagePackSerializerExtensions.DeserializeAsync<Request>(row["request"]);
					list.Add(request);
				}

				return list;
			}
			catch
			{
				if (transaction != null)
				{
					await transaction.RollbackAsync();
				}

				throw;
			}
		}

		public async Task<int> EnqueueAsync(IEnumerable<Request> requests)
		{
			MySqlTransaction transaction = null;
			try
			{
				await using var conn = new MySqlConnection(_options.ConnectionString);
				if (conn.State != ConnectionState.Open)
				{
					await conn.OpenAsync();
				}

				transaction = await conn.BeginTransactionAsync();

				var total = 0;
				foreach (var request in requests)
				{
					_requestHasher.ComputeHash(request);

					var data = new {request.Hash, Request = request.Serialize()};
					var cnt = await conn.ExecuteAsync(_insertSetSql, data, transaction);
					if (cnt > 0)
					{
						await conn.ExecuteAsync(_insertQueueSql, data, transaction);
						total += 1;
					}
				}

				await transaction.CommitAsync();

				return total;
			}
			catch
			{
				if (transaction != null)
				{
					await transaction.RollbackAsync();
				}

				throw;
			}
		}

		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		public async Task<long> GetTotalAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<long>(_totalSql);
		}

		public virtual async Task ResetDuplicateCheckAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(_resetDuplicateCheckSql);
		}

		public async Task CleanAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync($"DROP table `{_spiderId}_set`");
			await conn.ExecuteAsync($"DROP table `{_spiderId}_queue`");
		}

		public async Task ReloadAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$@"INSERT IGNORE INTO `{_spiderId}_queue` (hash, request) SELECT hash, request FROM `{_spiderId}_set` where state = 'P'");
		}

		public async Task SuccessAsync(Request request)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(_successSql, new {request.Hash});
		}

		public async Task FailAsync(Request request)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(_failSql, new {request.Hash});
		}

		public void Dispose()
		{
		}
	}
}
