using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Broker.Services
{
	public class RequestQueueService : BaseService, IRequestQueueService
	{
		public RequestQueueService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		public async Task Add(IEnumerable<RequestQueue> requests)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync($@"INSERT INTO requestqueue (requestid,{LeftEscapeSql}identity{RightEscapeSql},
blockid,request,response,statuscode,creationtime) VALUES (@RequestId,@Identity,@BlockId,@Request,@Response,0,{GetDateSql});", requests);

			}
		}

		/// <summary>
		/// 添加请求到队列
		/// </summary>
		/// <param name="json">Request 数组的序列化</param>
		/// <param name="identity">实例标识</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public async Task<string> Add(string identity, string json)
		{
			var requests = JsonConvert.DeserializeObject<List<Request>>(json);

			var blockId = Guid.NewGuid().ToString("N");
			var list = new List<RequestQueue>();
			foreach (var request in requests)
			{
				list.Add(new RequestQueue
				{
					BlockId = blockId,
					Identity = identity,
					Request = JsonConvert.SerializeObject(request),
					RequestId = request.Identity
				});
			}
			await Add(list);
			return blockId;
		}

		public async Task<IEnumerable<RequestQueue>> GetByBlockId(string blockId)
		{
			using (var conn = CreateDbConnection())
			{
				return await conn.QueryAsync<RequestQueue>($"SELECT * FROM requestqueue WHERE blockid = @BlockId", new { BlockId = blockId });
			}
		}
	}
}
