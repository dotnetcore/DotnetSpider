using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Agent.Message;
using DotnetSpider.AgentRegister.Store;
using DotnetSpider.Extensions;
using DotnetSpider.Infrastructure;
using DotnetSpider.Portal.Common;
using DotnetSpider.Portal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftMQ;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/agents")]
	public class AgentController : Controller
	{
		private readonly PortalDbContext _dbContext;
		private readonly IMessageQueue _mq;
		private readonly SpiderOptions _options;

		public AgentController(PortalDbContext dbContext,
			IMessageQueue eventBus,
			IOptions<SpiderOptions> options)
		{
			_dbContext = dbContext;
			_mq = eventBus;
			_options = options.Value;
		}

		public async Task<PagedQueryResult<AgentInfo>> PagedQueryAsync(string keyword, int page, int limit)
		{
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				var agents = await _dbContext
					.Set<AgentInfo>()
					.PagedQueryAsync(page, limit, x => x.Name.Contains(keyword) || x.Id.Contains(keyword),
						new OrderCondition<AgentInfo, DateTimeOffset>(x => x.CreationTime));
				return agents;
			}
			else
			{
				var agents = await _dbContext
					.Set<AgentInfo>()
					.PagedQueryAsync(page, limit, null,
						new OrderCondition<AgentInfo, DateTimeOffset>(x => x.CreationTime));
				return agents;
			}
		}

		[HttpGet("{id}/heartbeats")]
		public async Task<PagedQueryResult<AgentHeartbeat>> Heartbeat(string id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 20 ? 20 : size;

			return await _dbContext.Set<AgentHeartbeat>().PagedQueryAsync(page, size, x => x.AgentId == id,
				new OrderCondition<AgentHeartbeat, int>(x => x.Id));
		}

		[HttpDelete("{id}")]
		public async Task<IApiResult> DeleteAsync(string id)
		{
			if (!await _dbContext.Set<AgentInfo>().AnyAsync(x => x.Id == id))
			{
				return new FailedResult("Agent is not exists");
			}

			await _mq.PublishAsBytesAsync(string.Format(TopicNames.Agent, id.ToUpper()), new Exit {Id = id});

			using (var conn = _dbContext.Database.GetDbConnection())
			{
				await conn.ExecuteAsync(
					$"DELETE FROM {_options.Database}.agent_heartbeat WHERE agent_id = @Id; DELETE FROM {_options.Database}.agent_heartbeat WHERE id = @Id;",
					new {Id = id});
			}

			return new ApiResult("OK");
		}

		[HttpPut("{id}/exit")]
		public async Task<IApiResult> ExitAsync(string id)
		{
			if (!await _dbContext.Set<AgentInfo>().AnyAsync(x => x.Id == id))
			{
				return new FailedResult("Agent is not exists");
			}

			await _mq.PublishAsBytesAsync(string.Format(TopicNames.Agent, id.ToUpper()), new Exit {Id = id});
			return new ApiResult("OK");
		}
	}
}
