using Dapper;
using DotnetSpider.Common.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services.MySql
{
	public class BlockService : Services.BlockService
	{
		public BlockService(BrokerOptions options, IRunningService runningService,
			IRequestQueueService requestQueueService, ILogger<BlockService> logger) : base(options, runningService, requestQueueService, logger)
		{
		}

		public override async Task<Block> GetOneCompletedByIdentity(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				var block = await conn.QueryFirstOrDefaultAsync<Block>($"SELECT * FROM block WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity AND status=@Status LIMIT 1;",
					new
					{
						Identity = identity,
						Status = BlockStatus.Success
					});
				return block;
			}
		}

		protected override string LeftEscapeSql => "`";

		protected override string RightEscapeSql => "`";

		protected override string GetDateSql => "current_timestamp()";
	}
}
