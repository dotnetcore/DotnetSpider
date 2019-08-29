using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.MessageQueue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class DownloaderAgentController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly IMq _mq;

		public DownloaderAgentController(PortalDbContext dbContext, IMq eventBus,
			ILogger<DownloaderAgentController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_mq = eventBus;
		}

		[HttpGet("downloader-agent")]
		public async Task<IActionResult> Retrieve()
		{
			try
			{
				var agents = await _dbContext.Set<DownloaderAgent>().ToListAsync();
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

		[HttpGet("downloader-agent/{id}/heartbeat")]
		public async Task<IActionResult> Heartbeat(string id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 20 ? 20 : size;

			try
			{
				using (var conn = _dbContext.Database.GetDbConnection())
				{
//					var heartbeats = conn.QueryAsync<DownloaderAgentHeartbeat>(
//						$"select * from `dotnetspider`.`downloader_agent_heartbeat` where agent_id = @AgentId limit {(page - 1) * size},{size}",
//						new {AgentId = id});

					var viewModel = await _dbContext.Set<DownloaderAgentHeartbeat>().Where(x => x.AgentId == id)
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

		[HttpDelete("downloader-agent/{id}")]
		public async Task<IActionResult> DeleteAsync(string id)
		{
			if (!await _dbContext.Set<DownloaderAgent>().AnyAsync(x => x.Id == id))
			{
				return NotFound();
			}

			await _mq.PublishAsync(id, new MessageData<string> {Type = Framework.ExitCommand, Data = id});

			using (var conn = _dbContext.Database.GetDbConnection())
			{
				await conn.ExecuteAsync(
					"DELETE FROM downloader_agent_heartbeat WHERE agent_id = @Id; DELETE FROM downloader_agent WHERE id = @Id;",
					new {Id = id});
			}

			return Ok();
		}

		[HttpPost("downloader-agent/{id}/exit")]
		public async Task<IActionResult> ExitAsync(string id)
		{
			if (!await _dbContext.Set<DownloaderAgent>().AnyAsync(x => x.Id == id))
			{
				return NotFound();
			}

			await _mq.PublishAsync(id, new MessageData<string> {Type = Framework.ExitCommand, Data = id});
			return Ok();
		}
	}
}
