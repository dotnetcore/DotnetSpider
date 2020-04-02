using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Agent.Message;
using DotnetSpider.AgentRegister.Store;
using DotnetSpider.Extensions;
using DotnetSpider.Portal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftMQ;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class AgentController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly IMessageQueue _mq;
		private readonly SpiderOptions _options;

		public AgentController(PortalDbContext dbContext,
			IMessageQueue eventBus,
			IOptions<SpiderOptions> options,
			ILogger<AgentController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_mq = eventBus;
			_options = options.Value;
		}

		[HttpGet("agent")]
		public async Task<IActionResult> Retrieve()
		{
			try
			{
				var agents = await _dbContext.Set<AgentInfo>().ToListAsync();
				return View(agents);
			}
			catch (Exception e)
			{
				if (e.Message.Contains("doesn't exist"))
				{
					_logger.LogInformation("下载中心尚未启动");
					return View();
				}

				throw;
			}
		}

		[HttpGet("agent/{id}/heartbeat")]
		public async Task<IActionResult> Heartbeat(string id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 20 ? 20 : size;

			try
			{
				using (var conn = _dbContext.Database.GetDbConnection())
				{
					var viewModel = await _dbContext.Set<AgentHeartbeat>().Where(x => x.AgentId == id)
						.OrderByDescending(x => x.Id)
						.ToPagedListAsync(page, size);

					return View(viewModel);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("doesn't exist"))
				{
					_logger.LogInformation("下载中心尚未启动");
					return View();
				}

				throw;
			}
		}

		[HttpDelete("agent/{id}")]
		public async Task<IActionResult> DeleteAsync(string id)
		{
			if (!await _dbContext.Set<AgentInfo>().AnyAsync(x => x.Id == id))
			{
				return NotFound();
			}

			await _mq.PublishAsBytesAsync(string.Format(TopicNames.Agent, id.ToUpper()), new Exit {Id = id});

			using (var conn = _dbContext.Database.GetDbConnection())
			{
				await conn.ExecuteAsync(
					$"DELETE FROM {_options.Database}.agent_heartbeat WHERE agent_id = @Id; DELETE FROM {_options.Database}.agent_heartbeat WHERE id = @Id;",
					new {Id = id});
			}

			return Ok();
		}

		[HttpPost("agent/{id}/exit")]
		public async Task<IActionResult> ExitAsync(string id)
		{
			if (!await _dbContext.Set<AgentInfo>().AnyAsync(x => x.Id == id))
			{
				return NotFound();
			}

			await _mq.PublishAsBytesAsync(string.Format(TopicNames.Agent, id.ToUpper()), new Exit {Id = id});
			return Ok();
		}
	}
}
