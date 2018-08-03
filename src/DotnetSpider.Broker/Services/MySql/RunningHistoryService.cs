using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Common.Entity;
using Dapper;

namespace DotnetSpider.Broker.Services.MySql
{
	public class RunningHistoryService : BaseService, IRunningHistoryService
	{
		protected RunningHistoryService(BrokerOptions options) : base(options)
		{
		}

		public async Task Add(RunningHistory history)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("INSERT INTO running_history (identity,priority) VALUES (@Identity, @Priority)", history);
			}
		}

		public async Task Delete(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("DELETE FROM running_history WHERE identity = @Identity", identity);
			}
		}
	}
}
