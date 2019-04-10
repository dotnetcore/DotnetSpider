using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Downloader.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class DownloaderAgentController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;

		public DownloaderAgentController(PortalDbContext dbContext,
			ILogger<DockerController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
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
				else
				{
					throw;
				}
			}
		}

		[HttpGet("downloader-agent/{id}/heartbeat")]
		public async Task<IActionResult> Heartbeat(string id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 10 ? 10 : size;

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
				else
				{
					throw;
				}
			}
		}
	}
}