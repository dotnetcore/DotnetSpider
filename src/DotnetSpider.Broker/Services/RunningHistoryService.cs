using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Common.Entity;
using Dapper;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Services
{
	public class RunningHistoryService : BaseService, IRunningHistoryService
	{
		public RunningHistoryService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		public virtual async Task Add(RunningHistory history)
		{
			using (var conn = CreateDbConnection())
			{
				//  第一次添加则默认为 Init 状态
				history.Status = Common.Status.Init;
				await conn.ExecuteAsync($"INSERT INTO runninghistory ({LeftEscapeSql}identity{RightEscapeSql},jobid,priority,status,creationtime) VALUES (@Identity,@JobId, @Priority,@Status,{GetDateSql})", history);
			}
		}

		public virtual async Task Delete(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($"DELETE FROM runninghistory WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity", new { Identity = identity });
			}
		}

		public virtual async Task Update(RunningHistory history)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($"UPDATE runninghistory SET status=@Status, lastmodificationtime = {GetDateSql} WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity", new { history.Identity });
			}
		}
	}
}
