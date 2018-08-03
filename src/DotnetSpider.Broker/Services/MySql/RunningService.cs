using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Common.Entity;
using Dapper;

namespace DotnetSpider.Broker.Services.MySql
{
	public class RunningService : BaseService, IRunningService
	{
		protected RunningService(BrokerOptions options) : base(options)
		{
		}

		public async Task Add(Running history)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("INSERT INTO running (identity,priority) VALUES (@Identity, @Priority)", history);
			}
		}

		public async Task Delete(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("DELETE FROM running WHERE identity = @Identity", identity);
			}
		}

		public async Task<List<Running>> Get(string[] runnings)
		{
			using (var conn = CreateDbConnection())
			{
				var identities = string.Join(',', runnings.Select(r => $"'{r}'"));
				return (await conn.QueryAsync<Running>($"SELECT * FROM running WHERE identity NOT IN ({identities})")).ToList();
			}
		}

		public async Task<List<Running>> GetAll()
		{
			using (var conn = CreateDbConnection())
			{
				return (await conn.QueryAsync<Running>("SELECT * FROM running")).ToList();
			}
		}

		public async Task Update(Running running)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("UPDATE running SET priority = @Priority, last_modification_time = current_timestamp() WHERE identity = @Identity", running);
			}
		}
	}
}
