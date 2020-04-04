using System;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using DotnetSpider.Agent.Message;
using DotnetSpider.AgentRegister.Store;
using DotnetSpider.Extensions;
using DotnetSpider.Infrastructure;
using DotnetSpider.Portal.Common;
using DotnetSpider.Portal.Data;
using DotnetSpider.Portal.ViewObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SwiftMQ;

namespace DotnetSpider.Portal.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/agents")]
	public class AgentController : Controller
	{
		private readonly PortalDbContext _dbContext;
		private readonly IMessageQueue _mq;
		private readonly SpiderOptions _options;
		private readonly IMapper _mapper;

		public AgentController(PortalDbContext dbContext,
			IMessageQueue eventBus,
			IOptions<SpiderOptions> options, IMapper mapper)
		{
			_dbContext = dbContext;
			_mq = eventBus;
			_mapper = mapper;
			_options = options.Value;
		}

		[HttpGet]
		public async Task<PagedQueryResult<AgentViewObject>> PagedQueryAsync(string keyword, int page, int limit)
		{
			PagedQueryResult<AgentInfo> @out;
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				@out = await _dbContext
					.Set<AgentInfo>()
					.PagedQueryAsync(page, limit, x => x.Name.Contains(keyword) || x.Id.Contains(keyword),
						new OrderCondition<AgentInfo, DateTimeOffset>(x => x.CreationTime));
			}
			else
			{
				@out = await _dbContext
					.Set<AgentInfo>()
					.PagedQueryAsync(page, limit, null,
						new OrderCondition<AgentInfo, DateTimeOffset>(x => x.CreationTime));
			}

			return _mapper.ToPagedQueryResult<AgentInfo, AgentViewObject>(@out);
		}

		[HttpGet("{id}/heartbeats")]
		public async Task<PagedQueryResult<AgentHeartbeatViewObject>> PagedQueryHeartbeatAsync(string id, int page, int limit)
		{
			page = page <= 1 ? 1 : page;
			limit = limit <= 5 ? 5 : limit;

			var @out = await _dbContext.Set<AgentHeartbeat>().PagedQueryAsync(page, limit, x => x.AgentId == id,
				new OrderCondition<AgentHeartbeat, int>(x => x.Id));
			return _mapper.ToPagedQueryResult<AgentHeartbeat, AgentHeartbeatViewObject>(@out);
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
					$"DELETE FROM {_options.Database}.agent_heartbeat WHERE agent_id = @Id; DELETE FROM {_options.Database}.agent WHERE id = @Id;",
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
