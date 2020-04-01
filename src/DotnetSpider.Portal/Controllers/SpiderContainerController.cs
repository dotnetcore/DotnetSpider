using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Agent.Message;
using DotnetSpider.Extensions;
using DotnetSpider.Portal.Models.SpiderContainer;
using DotnetSpider.Statistics.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwiftMQ;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderContainerController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly IMessageQueue _mq;

		public SpiderContainerController(PortalDbContext dbContext,
			IMessageQueue eventBus,
			ILogger<SpiderController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_mq = eventBus;
		}

		[HttpGet("spider/{id}/containers")]
		public async Task<IActionResult> Retrieve(int id, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 20 ? 20 : size;

			var containers = await _dbContext.SpiderContainers.Where(x => x.SpiderId == id)
				.OrderByDescending(x => x.Id)
				.ToPagedListAsync(page, size);

			var batches = await containers.Select(x => x.Batch).ToListAsync();
			var dict = await _dbContext.Set<SpiderStatistics>().Where(x => batches.Contains(x.Id))
				.ToDictionaryAsync(x => x.Id, x => x);

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
					item.Failed = dict[item.Batch].Failure;
					item.Success = dict[item.Batch].Success;
					item.Start = dict[item.Batch].Start;
					item.Exit = dict[item.Batch].Exit;
					item.Left = item.Total - item.Success;
				}

				list.Add(item);
			}

			return View(new StaticPagedList<ListSpiderContainerViewModel>(list, page, size,
				containers.GetMetaData().TotalItemCount));
		}

		[HttpPost("spider/{batch}/exit")]
		public async Task<IActionResult> ExitAsync(string batch)
		{
			try
			{
				await _mq.PublishAsBytesAsync(string.Format(TopicNames.Spider, batch.ToUpper()), new Exit {Id = batch});
				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogError($"关闭失败: {e}");
				return StatusCode((int)HttpStatusCode.InternalServerError, new {e.Message});
			}
		}
	}
}
