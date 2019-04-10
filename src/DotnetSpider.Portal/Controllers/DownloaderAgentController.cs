using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Downloader.Entity;
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

		public DownloaderAgentController(PortalDbContext dbContext, ILogger<DockerController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		[HttpGet("downloader-agent")]
		public async Task<IActionResult> Retrieve()
		{
			using (var conn = _dbContext.Database.GetDbConnection())
			{
				var agents = await (await conn.QueryAsync<DownloaderAgent>(
						$"SELECT id AS Id, name AS Name, processor_count AS ProcessorCount, total_memory AS TotalMemory, creation_time AS CreationTime, last_modification_time AS LastModificationTime FROM downloader_agent")
					)
					.ToListAsync();
				return View(agents);
			}
		}

		[HttpGet("downloader-agent/{id}/heartbeat")]
		public async Task<IActionResult> Heartbeat(string id, int page, int size)
		{
			using (var conn = _dbContext.Database.GetDbConnection())
			{
				page = page <= 1 ? 1 : page;
				size = size <= 10 ? 10 : size;

				var agents = await (await conn.QueryAsync<DownloaderAgentHeartbeat>(
						$"SELECT id AS Id, agent_id AS AgentId, agent_name AS AgentName, free_memory AS FreeMemory, downloader_count AS DownloaderCount, creation_time AS CreationTime FROM downloader_agent_heartbeat WHERE agent_id = @AgentId ORDER BY id DESC LIMIT @Page, @Size ",
						new
						{
							AgentId = id,
							Page = page - 1,
							Size = size
						})
					)
					.ToListAsync();
				var viewModel = new StaticPagedList<DownloaderAgentHeartbeat>(agents, page, size, 10000);
				return View(viewModel);
			}
		}
	}
}