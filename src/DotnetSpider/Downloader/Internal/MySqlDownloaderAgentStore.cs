using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Downloader.Entity;
using MySql.Data.MySqlClient;
using Dapper;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader.Internal
{
	public class MySqlDownloaderAgentStore : IDownloaderAgentStore
	{
		private readonly ISpiderOptions _options;

		public MySqlDownloaderAgentStore(ISpiderOptions options)
		{
			_options = options;
		}

		public async Task EnsureDatabaseAndTableCreatedAsync()
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync("CREATE SCHEMA IF NOT EXISTS dotnetspider DEFAULT CHARACTER SET utf8mb4;");
				var sql1 =
					$"create table if not exists dotnetspider.downloader_agent(id nvarchar(40) primary key, `name` nvarchar(255) null, processor_count int null, total_memory int null, creation_time timestamp default CURRENT_TIMESTAMP not null, last_modification_time timestamp default CURRENT_TIMESTAMP not null, key NAME_INDEX (`name`));";
				var sql2 =
					$"create table if not exists dotnetspider.downloader_agent_heartbeat(id bigint AUTO_INCREMENT primary key, agent_id nvarchar(40) not null, `agent_name` nvarchar(255) null, free_memory int null, downloader_count int null, creation_time timestamp default CURRENT_TIMESTAMP not null, key NAME_INDEX (`agent_name`), key ID_INDEX (`agent_id`));";
				var sql3 =
					$"create table if not exists dotnetspider.downloader_agent_allocate(id bigint AUTO_INCREMENT primary key, owner_id nvarchar(40) not null, agent_id nvarchar(40) not null, creation_time timestamp default CURRENT_TIMESTAMP not null, key OWNER_ID_INDEX (`owner_id`), unique key OWNER_ID_AGENT_ID_INDEX (owner_id, agent_id));";
				var sql4 =
					$"create table if not exists dotnetspider.allocate_downloader_message(id bigint AUTO_INCREMENT primary key, owner_id nvarchar(40) not null, `message` text not null, creation_time timestamp default CURRENT_TIMESTAMP not null, key OWNER_ID_INDEX (`owner_id`));";
				await conn.ExecuteAsync(sql1);
				await conn.ExecuteAsync(sql2);
				await conn.ExecuteAsync(sql3);
				await conn.ExecuteAsync(sql4);
			}
		}

		public async Task<List<DownloaderAgent>> GetAllListAsync()
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				return (await conn.QueryAsync<DownloaderAgent>(
						$"SELECT * FROM dotnetspider.downloader_agent"))
					.ToList();
			}
		}

		public async Task<List<DownloaderAgentAllocate>> GetAllocatedListAsync(string ownerId)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				return (await conn.QueryAsync<DownloaderAgentAllocate>(
					$"SELECT owner_id as OwnerId, agent_id as AgentId, id FROM dotnetspider.downloader_agent_allocate WHERE owner_id = @OwnerId",
					new {OwnerId = ownerId})).ToList();
			}
		}

		public async Task RegisterAsync(DownloaderAgent agent)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(
					$"INSERT IGNORE INTO dotnetspider.downloader_agent (id, `name`, processor_count, total_memory, creation_time, last_modification_time) VALUES (@Id, @Name, @ProcessorCount, @TotalMemory, @CreationTime, @LastModificationTime);",
					agent);
			}
		}

		public async Task HeartbeatAsync(DownloaderAgentHeartbeat agent)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				await conn.ExecuteAsync(
					$"INSERT IGNORE INTO dotnetspider.downloader_agent_heartbeat (agent_id, agent_name, free_memory, downloader_count, creation_time) VALUES (@AgentId, @AgentName, @FreeMemory, @DownloaderCount, @CreationTime);",
					agent);
				await conn.ExecuteAsync(
					$"UPDATE dotnetspider.downloader_agent SET last_modification_time = @LastModificationTime WHERE id = @AgentId",
					new {agent.AgentId, LastModificationTime = agent.CreationTime});
			}
		}

		/// <summary>
		/// 保存给任务分配的下载器代理
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <param name="message">分配下载器代理的消息</param>
		/// <param name="agentIds">分配的下载器代理标识</param>
		/// <returns></returns>
		public async Task AllocateAsync(string ownerId, string message, IEnumerable<string> agentIds)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				var data = agentIds.Select(x => new {AgentId = x, OwnerId = ownerId}).ToList();
				await conn.ExecuteAsync(
					$"INSERT IGNORE INTO dotnetspider.downloader_agent_allocate (owner_id, agent_id) VALUES (@OwnerId, @AgentId);",
					data);
				await conn.ExecuteAsync(
					$"INSERT IGNORE INTO dotnetspider.allocate_downloader_message (owner_id, `message`) VALUES (@OwnerId, @Message);",
					new {OwnerId = ownerId, Message = message});
			}
		}

		public async Task<string> GetAllocateDownloaderMessageAsync(string ownerId)
		{
			using (var conn = new MySqlConnection(_options.ConnectionString))
			{
				return await conn.QuerySingleAsync<string>(
					$"SELECT `message` FROM dotnetspider.allocate_downloader_message WHERE owner_id = @OwnerId",
					new {OwnerId = ownerId});
			}
		}
	}
}