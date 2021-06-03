using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.AgentCenter;
using DotnetSpider.AgentCenter.Store;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace DotnetSpider.MySql.AgentCenter
{
	public class MySqlAgentStore : IAgentStore
	{
		private readonly AgentCenterOptions _options;

		public MySqlAgentStore(IOptions<AgentCenterOptions> options)
		{
			_options = options.Value;
		}

		public async Task EnsureDatabaseAndTableCreatedAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$"CREATE SCHEMA IF NOT EXISTS {_options.Database} DEFAULT CHARACTER SET utf8mb4;");
			var sql1 =
				$"create table if not exists {_options.Database}.agent (id varchar(36) primary key, `name` varchar(255) null, processor_count int null, total_memory int null, is_deleted tinyint(1) default 0 null, creation_time timestamp default CURRENT_TIMESTAMP not null, last_modification_time timestamp default CURRENT_TIMESTAMP not null, key NAME_INDEX (`name`));";
			var sql2 =
				$"create table if not exists {_options.Database}.agent_heartbeat(id bigint AUTO_INCREMENT primary key, agent_id varchar(36) not null, `agent_name` varchar(255) null, cpu_load int, available_memory int null, creation_time timestamp default CURRENT_TIMESTAMP not null, key NAME_INDEX (`agent_name`), key ID_INDEX (`agent_id`));";
			await conn.ExecuteAsync(sql1);
			await conn.ExecuteAsync(sql2);
		}

		public async Task<IEnumerable<AgentInfo>> GetAllListAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.QueryAsync<AgentInfo>(
				$"SELECT * FROM {_options.Database}.agent"));
		}

		public async Task RegisterAsync(AgentInfo agent)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$"INSERT IGNORE INTO {_options.Database}.agent (id, `name`, processor_count, total_memory, creation_time, last_modification_time) VALUES (@Id, @Name, @ProcessorCount, @TotalMemory, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP); UPDATE {_options.Database}.agent SET is_deleted = false WHERE id = @Id",
				new {agent.Id, agent.Name, agent.ProcessorCount, agent.TotalMemory});
		}

		public async Task HeartbeatAsync(AgentHeartbeat heartbeat)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$"INSERT IGNORE INTO {_options.Database}.agent_heartbeat (agent_id, agent_name, available_memory, cpu_load, creation_time) VALUES (@AgentId, @AgentName, @FreeMemory, @CpuLoad, CURRENT_TIMESTAMP);",
				new {heartbeat.AgentId, heartbeat.AgentName, heartbeat.AvailableMemory, heartbeat.CpuLoad});
			await conn.ExecuteAsync(
				$"UPDATE {_options.Database}.agent SET last_modification_time = CURRENT_TIMESTAMP WHERE id = @AgentId",
				new {heartbeat.AgentId,});
		}
	}
}
