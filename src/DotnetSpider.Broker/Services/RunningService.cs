using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Common.Entity;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace DotnetSpider.Broker.Services
{
	public class RunningService : BaseService, IRunningService
	{
		public RunningService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		public virtual async Task Add(Running history)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($"INSERT INTO running ({LeftEscapeSql}identity{RightEscapeSql},priority,blocktimes,creationtime) VALUES (@Identity, @Priority,@BlockTimes,{GetDateSql})", history);
				await conn.ExecuteAsync($"INSERT INTO runninghistory ({LeftEscapeSql}identity{RightEscapeSql},priority,creationtime) VALUES (@Identity, @Priority,{GetDateSql})", history);
			}
		}

		public virtual async Task Delete(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($"DELETE FROM running WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity", new { Identity = identity });
			}
		}

		public virtual async Task<Running> Get(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				return (await conn.QueryFirstOrDefaultAsync<Running>($"SELECT * FROM running WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity", new
				{
					Identity = identity
				}));
			}
		}

		public virtual async Task<List<Running>> GetAll()
		{
			using (var conn = CreateDbConnection())
			{
				return (await conn.QueryAsync<Running>("SELECT * FROM running")).ToList();
			}
		}

		public virtual async Task Update(Running running)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($"UPDATE running SET priority = @Priority,blocktimes =@BlockTimes, lastmodificationtime = {GetDateSql} WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity", running);
			}
		}

		public async virtual Task<Running> Pop(IDbConnection conn, IDbTransaction transaction, string[] runnings)
		{
			var where = runnings == null || runnings.Length == 0 ? "" : $"WHERE {LeftEscapeSql}identity{RightEscapeSql} NOT IN ({string.Join(',', runnings.Select(r => $"'{r}'"))})";
			var running = (await conn.QueryFirstOrDefaultAsync<Running>(
				$"SELECT TOP 1 * FROM running {where}ORDER BY Priority DESC, BlockTimes ASC", null, transaction));
			if (running != null)
			{
				running.BlockTimes += 1;
			}
			await conn.ExecuteAsync(
				$"UPDATE running SET blocktimes =@BlockTimes WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity",
				new { running.Identity, running.BlockTimes }, transaction);

			return running;
		}
	}
}
