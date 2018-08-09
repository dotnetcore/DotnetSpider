using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.Common.Dto;
using Dapper;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using DotnetSpider.Common.Entity;
using System.Data;

namespace DotnetSpider.Broker.Services
{
	public class BlockService : BaseService, IBlockService
	{
		private readonly IRunningService _runningService;
		private readonly IRequestQueueService _requestQueueService;

		public BlockService(BrokerOptions options, IRunningService runningService,
			IRequestQueueService requestQueueService, ILogger<BlockService> logger) : base(options, logger)
		{
			_runningService = runningService;
			_requestQueueService = requestQueueService;
		}

		public async Task Add(Block block)
		{
			block.Status = BlockStatus.Ready;
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($@"INSERT INTO block (blockid,{LeftEscapeSql}identity{RightEscapeSql},status,
creationtime) VALUES (@BlockId,@Identity,@Status,{GetDateSql})", block);
			}
		}

		public async Task<BlockOutput> Pop(NodeHeartbeatInput heartbeat)
		{
			using (var conn = CreateDbConnection())
			{
				if (conn.State == ConnectionState.Closed)
				{
					conn.Open();
				}
				var transaction = conn.BeginTransaction();
				BlockOutput output = new BlockOutput { Command = Command.Download };
				try
				{
					var running = await _runningService.Pop(conn, transaction, heartbeat.Runnings);
					output.Identity = running.Identity;

					var block = await conn.QueryFirstOrDefaultAsync<Block>($@"SELECT TOP 1 * FROM block WHERE
{LeftEscapeSql}identity{RightEscapeSql}=@Identity WHERE status = 1 or status = 5", new { running.Identity }, transaction);
					if (block != null)
					{
						await conn.ExecuteAsync($"UPDATE block SET status =@Status, lastmodificationtime={GetDateSql} WHERE blockid=@BlockId",
							new { Status = BlockStatus.Using, block.BlockId }, transaction);
						var requests = await _requestQueueService.GetByBlockId(block.BlockId);
						output.Requests = requests.Select(r =>
						{
							var request = JsonConvert.DeserializeObject<Request>(r.Request);
							return new RequestOutput { Content = request.Content, CycleTriedTimes = request.CycleTriedTimes, Depth = request.Depth, Method = request.Method, Origin = request.Origin, Referer = request.Referer, Url = request.Url };
						}).ToList();
						output.BlockId = block.BlockId;
						output.Site = JsonConvert.DeserializeObject<Site>(running.Site);
						output.ThreadNum = running.ThreadNum;
					}
					transaction.Commit();
				}
				catch
				{
					try
					{
						transaction.Rollback();
					}
					catch (Exception ex)
					{
						_logger.LogError(ex.ToString());
					}
					throw;
				}
				return output;
			}
		}

		public async Task Callback(BlockInput input)
		{
			using (var conn = CreateDbConnection())
			{
				var list = new List<dynamic>();
				foreach (var result in input.Results)
				{
					list.Add(new
					{
						Response = JsonConvert.SerializeObject(result),
						result.StatusCode,
						result.ResponseTime,
						RequestId = result.Identity,
						input.Identity
					});
				}
				await conn.ExecuteAsync($"UPDATE requestqueue SET response = @Response, statuscode = @StatusCode, responsetime = @ResponseTime WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity AND requestid = @RequestId", list);
				await Update(new Block { BlockId = input.BlockId, Exception = input.Exception, Identity = input.Identity, Status = string.IsNullOrWhiteSpace(input.Exception) ? BlockStatus.Success : BlockStatus.Failed });
			}
		}

		public async Task Update(Block block)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($@"UPDATE block SET exception = @Exceptoin, success = @Success,
lastmodificationtime={GetDateSql} WHERE blockid=@BlockId", block);
			}
		}

		public virtual async Task<Block> GetOneCompletedByIdentity(string identity)
		{
			using (var conn = CreateDbConnection())
			{
				var block = await conn.QueryFirstOrDefaultAsync<Block>($"SELECT TOP 1 * FROM block WHERE {LeftEscapeSql}identity{RightEscapeSql} = @Identity AND status=@Status;",
					new
					{
						Identity = identity,
						Status = BlockStatus.Success
					});
				return block;
			}
		}
	}
}
