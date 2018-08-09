using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common.Entity;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Services.MySql
{
	public class RunningService : Broker.Services.RunningService
	{
		public RunningService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		public override async Task<Running> Pop(IDbConnection conn, IDbTransaction transaction, string[] runnings)
		{
			var where = runnings == null || runnings.Length == 0 ? "" : $"WHERE {LeftEscapeSql}identity{RightEscapeSql} NOT IN ({string.Join(',', runnings.Select(r => $"'{r}'"))})";
			var running = (await conn.QueryFirstOrDefaultAsync<Running>(
				$"SELECT * FROM running {where}ORDER BY Priority DESC, BlockTimes ASC LIMIT 1", null, transaction));
			if (running != null)
			{
				running.BlockTimes += 1;
			}
			await conn.ExecuteAsync(
				$"UPDATE running SET blocktimes =@BlockTimes WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity",
				new { running.Identity, running.BlockTimes }, transaction);

			return running;
		}

		protected override string LeftEscapeSql => "`";

		protected override string RightEscapeSql => "`";

		protected override string GetDateSql => "current_timestamp()";
	}
}
