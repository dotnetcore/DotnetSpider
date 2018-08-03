using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.Common.Dto;
using Dapper;
using Newtonsoft.Json;

namespace DotnetSpider.Broker.Services.MySql
{
	public class BlockService : BaseService, IBlockService
	{
		private readonly IRunningService _runningService;

		public BlockService(BrokerOptions options, IRunningService runningService) : base(options)
		{
			_runningService = runningService;
		}

		public Task<BlockOutput> Pull(NodeHeartbeatInput heartbeat)
		{
			var allJobs = _runningService.GetAll();
			return Task.FromResult(new BlockOutput
			{
				Command = Command.Download,
				Id = "1",
				Identity = "1",
				Site = new Site(),
				ThreadNum = 1,
				Requests = new List<RequestOutput>
					{
						new RequestOutput  { Url="http://cnblogs.com", Method=HttpMethod.Get }
					}
			});
		}

		public async Task Push(BlockInput input)
		{
			using (var conn = CreateDbConnection())
			{
				var list = new List<dynamic>();
				foreach (var result in input.Results)
				{
					list.Add(new
					{
						Response = JsonConvert.SerializeObject(result),
						result.HttpStatusCode,
						result.ResponseTime
					});
				}
				await conn.ExecuteAsync($"UPDATE request_history SET response = @Response, status_code = @StatusCode, response_time = @ResonseTime WHERE identity = @Identity AND md5 = @MD5", list);
			}
		}
	}
}
