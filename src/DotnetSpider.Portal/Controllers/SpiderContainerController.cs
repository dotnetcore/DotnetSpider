using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.EventBus;
using DotnetSpider.Portal.Models.SpiderContainer;
using DotnetSpider.Statistics.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderContainerController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly Quartz.IScheduler _sched;
		private readonly PortalOptions _options;
		private readonly IEventBus _eventBus;

		public SpiderContainerController(PortalDbContext dbContext,
			PortalOptions options,
			Quartz.IScheduler sched,
			IEventBus eventBus,
			ILogger<SpiderController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_sched = sched;
			_options = options;
			_eventBus = eventBus;
		}

		[HttpGet("spider/{id}/containers")]
		public async Task<IActionResult> Retrieve(int id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 20 ? 20 : size;

			var containers = await _dbContext.SpiderContainers.Where(x => x.SpiderId == id)
				.OrderByDescending(x => x.CreationTime)
				.ToPagedListAsync(page, size);

			var batches = await containers.Select(x => x.Batch).ToListAsync();
			var dict = await _dbContext.Set<SpiderStatistics>().Where(x => batches.Contains(x.OwnerId))
				.ToDictionaryAsync(x => x.OwnerId, x => x);

			var list = new List<ListSpiderContainerViewModel>();
			foreach (var container in containers)
			{
				var item = new ListSpiderContainerViewModel
				{
					Batch = container.Batch,
					ContainerId = container.ContainerId,
					SpiderId = container.SpiderId,
					Status = container.Status,
					CreationTime = container.CreationTime
				};
				if (dict.ContainsKey(item.Batch))
				{
					item.Total = dict[item.Batch].Total;
					item.Failed = dict[item.Batch].Failed;
					item.Success = dict[item.Batch].Success;
					item.Start = dict[item.Batch].Start;
					item.Exit = dict[item.Batch].Exit;
				}

				list.Add(item);
			}

			return View(new StaticPagedList<ListSpiderContainerViewModel>(list, page, size,
				containers.GetMetaData().TotalItemCount));
		}

		[HttpPost("spider/{id}/exit")]
		public async Task<IActionResult> ExitAsync(string batch)
		{
			try
			{
				await _eventBus.PublishAsync(batch, $"|{Framework.ExitCommand}|{batch}");
				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogError($"关闭失败: {e}");
				return StatusCode((int) HttpStatusCode.InternalServerError, new
				{
					e.Message
				});
			}
		}
	}
}