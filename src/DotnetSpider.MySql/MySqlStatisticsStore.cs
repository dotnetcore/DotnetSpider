using System;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Infrastructure;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace DotnetSpider.MySql
{
	public class MySqlStatisticsStore : IStatisticsStore
	{
		private readonly SpiderOptions _options;

		public MySqlStatisticsStore(IOptions<SpiderOptions> options)
		{
			_options = options.Value;
		}

		public async Task EnsureDatabaseAndTableCreatedAsync()
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(
					$"CREATE SCHEMA IF NOT EXISTS {_options.Database} DEFAULT CHARACTER SET utf8mb4;");
				var sql1 =
					$"create table if not exists {_options.Database}.statistics(id varchar(36) primary key, `name` varchar(255), `start` timestamp null, `exit` timestamp null, total bigint default 0 not null, success bigint default 0 not null, failure bigint default 0 not null, creation_time timestamp default CURRENT_TIMESTAMP not null, last_modification_time timestamp default CURRENT_TIMESTAMP not null, key CREATION_TIME_INDEX (`creation_time`), key LAST_MODIFICATION_TIME_INDEX (`last_modification_time`));";
				var sql2 =
					$"create table if not exists {_options.Database}.agent_statistics(id varchar(36) primary key, `name` varchar(255), success bigint default 0 not null, failure bigint default 0 not null, elapsed_milliseconds bigint default 0 not null, creation_time timestamp default CURRENT_TIMESTAMP not null, last_modification_time timestamp default CURRENT_TIMESTAMP not null, key CREATION_TIME_INDEX (`creation_time`), key LAST_MODIFICATION_TIME_INDEX (`last_modification_time`));";
				await conn.ExecuteAsync(sql1);
				await conn.ExecuteAsync(sql2);
			}
		}

		public async Task IncreaseTotalAsync(string id, long count)
		{
			var sql =
				$"INSERT INTO {_options.Database}.statistics (id, total) VALUES (@OwnerId, @Count) ON DUPLICATE key UPDATE total = total + @Count, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {OwnerId = id, Count = count});
			}
		}

		public async Task IncreaseSuccessAsync(string id)
		{
			var sql =
				$"INSERT INTO {_options.Database}.statistics (id) VALUES (@OwnerId) ON DUPLICATE key UPDATE success = success + 1, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {OwnerId = id});
			}
		}

		public async Task IncreaseFailureAsync(string id)
		{
			var sql =
				$"INSERT INTO {_options.Database}.statistics (id) VALUES (@OwnerId) ON DUPLICATE key UPDATE failure = failure + 1, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {OwnerId = id});
			}
		}

		public async Task StartAsync(string id, string name)
		{
			var sql =
				$"INSERT INTO {_options.Database}.statistics (id, `name`, start) VALUES (@OwnerId, @Name, @Start) ON DUPLICATE key UPDATE start = @Start, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {OwnerId = id, Start = DateTimeOffset.Now, Name = name});
			}
		}

		public async Task ExitAsync(string id)
		{
			var sql =
				$"INSERT INTO {_options.Database}.statistics (id, `exit`) VALUES (@OwnerId, @Exit) ON DUPLICATE key UPDATE `exit` = @Exit, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {OwnerId = id, Exit = DateTimeOffset.Now});
			}
		}

		public async Task RegisterAgentAsync(string agentId, string agentName)
		{
			var sql =
				$"INSERT INTO {_options.Database}.agent_statistics (`id`, `agent_name`, creation_time) VALUES (@AgentId, @AgentName, CURRENT_TIMESTAMP) ON DUPLICATE key UPDATE  agent_name = @AgentName, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {AgentId = agentId, AgentName = agentName});
			}
		}

		public async Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
		{
			var sql =
				$"INSERT INTO {_options.Database}.agent_statistics (id, elapsed_milliseconds, creation_time) VALUES (@AgentId, @ElapsedMilliseconds, CURRENT_TIMESTAMP) ON DUPLICATE key UPDATE success = success + 1, elapsed_milliseconds = elapsed_milliseconds + @ElapsedMilliseconds, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {AgentId = agentId, ElapsedMilliseconds = elapsedMilliseconds});
			}
		}

		public async Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
		{
			var sql =
				$"INSERT INTO {_options.Database}.agent_statistics (id, elapsed_milliseconds, creation_time) VALUES (@AgentId, @ElapsedMilliseconds, CURRENT_TIMESTAMP) ON DUPLICATE key UPDATE failure = failure + 1, elapsed_milliseconds = elapsed_milliseconds + @ElapsedMilliseconds, last_modification_time = CURRENT_TIMESTAMP;";
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(sql,
					new {AgentId = agentId, ElapsedMilliseconds = elapsedMilliseconds});
			}
		}

		public async Task<PagedQueryResult<AgentStatistics>> PagedQueryAgentStatisticsAsync(string agentId, int page,
			int limit)
		{
			if (page <= 0)
			{
				page = 1;
			}

			if (limit <= 0)
			{
				limit = 5;
			}

			if (limit > 30)
			{
				limit = 30;
			}

			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				var start = limit * (page - 1);
				var where = string.IsNullOrWhiteSpace(agentId) ? "" : " WHERE `id` = @AgentId";
				var count = await conn.QuerySingleAsync<int>(
					$"SELECT COUNT(*) FROM {_options.Database}.agent_statistics {where}");
				var result = (await conn.QueryAsync<AgentStatistics>(
					$"SELECT * FROM {_options.Database}.agent_statistics {where} ORDER BY creation_time LIMIT @Start, @Offset;",
					new {Start = start, Offfset = limit, AgentId = agentId}));
				return new PagedQueryResult<AgentStatistics>(page, limit, count, result);
			}
		}

		public async Task<AgentStatistics> GetAgentStatisticsAsync(string agentId)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				return await conn.QuerySingleOrDefaultAsync<AgentStatistics>(
					$"SELECT * FROM {_options.Database}.agent_statistics WHERE id = @AgentId LIMIT 1;",
					new {AgentId = agentId});
			}
		}

		public async Task<SpiderStatistics> GetSpiderStatisticsAsync(string id)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				return await conn.QuerySingleOrDefaultAsync<SpiderStatistics>(
					$"SELECT * FROM {_options.Database}.statistics WHERE owner_id = @OwnerId LIMIT 1;",
					new {OwnerId = id});
			}
		}

		public async Task<PagedQueryResult<SpiderStatistics>> PagedQuerySpiderStatisticsAsync(string keyword, int page,
			int limit)
		{
			if (page <= 0)
			{
				page = 1;
			}

			if (limit <= 0)
			{
				limit = 5;
			}

			if (limit > 30)
			{
				limit = 30;
			}

			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				var start = limit * (page - 1);
				var where = string.IsNullOrWhiteSpace(keyword) ? "" : " WHERE `name` like @Keyword";
				var count = await conn.QuerySingleAsync<int>(
					$"SELECT COUNT(*) FROM {_options.Database}.statistics {where}");
				var result = (await conn.QueryAsync<SpiderStatistics>(
					$"SELECT * FROM {_options.Database}.statistics {where} ORDER BY creation_time LIMIT @Start, @Offset;",
					new {Start = start, Offfset = limit, Keyword = $"%{keyword}%"}));
				return new PagedQueryResult<SpiderStatistics>(page, limit, count, result);
			}
		}
	}
}
