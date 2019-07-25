using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.EventBus;
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
		private readonly IEventBus _eventBus;

		public DownloaderAgentController(PortalDbContext dbContext, IEventBus eventBus,
			ILogger<DownloaderAgentController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_eventBus = eventBus;
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
				var viewModel = await _dbContext.Set<DownloaderAgentHeartbeat>().Where(x => x.AgentId == id)
					.OrderByDescending(x => x.CreationTime)
					.ToPagedListAsync(page, size);

				return View(viewModel);
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

			await _eventBus.PublishAsync(id, new Event
			{
				Type = Framework.ExitCommand,
				Data = id
			});

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

			await _eventBus.PublishAsync(id, new Event
			{
				Type = Framework.ExitCommand,
				Data = id
			});
			return Ok();
		}
	}
}