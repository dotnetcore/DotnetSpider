using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Portal.Entity;
using DotnetSpider.Portal.Models.Spider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly Quartz.IScheduler _sched;
		private readonly PortalOptions _options;

		public SpiderController(PortalDbContext dbContext,
			PortalOptions options,
			Quartz.IScheduler sched,
			ILogger<SpiderController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_sched = sched;
			_options = options;
		}

		[HttpGet("spider/add")]
		public IActionResult Add()
		{
			return View();
		}


		[HttpPost("spider/add")]
		public async Task<IActionResult> Add(AddSpiderViewModel dto)
		{
			if (!ModelState.IsValid)
			{
				return View("Add", dto);
			}

			var exists = await _dbContext.Spiders.AnyAsync(x =>
				x.Name == dto.Name);
			if (exists)
			{
				ModelState.AddModelError("Name", "名称已经存在");
			}

			var imageExists = await _dbContext.DockerImages.AnyAsync(x => x.Image == dto.Image);
			if (!imageExists)
			{
				ModelState.AddModelError("Image", "镜像不存在");
			}

			try
			{
				TriggerBuilder.Create().WithCronSchedule(dto.Cron).Build();
			}
			catch
			{
				ModelState.AddModelError("Cron", "CRON 表达式不正确");
			}

			if (ModelState.IsValid)
			{
				var transaction = await _dbContext.Database.BeginTransactionAsync();
				try
				{
					var spider = new Portal.Entity.Spider
					{
						Name = dto.Name,
						Cron = dto.Cron,
						Image = dto.Image,
						Environment = dto.Environment,
						CreationTime = DateTime.Now,
						LastModificationTime = DateTime.Now
					};
					_dbContext.Spiders.Add(spider);
					await _dbContext.SaveChangesAsync();
					var id = spider.Id.ToString();
					var trigger = TriggerBuilder.Create().WithCronSchedule(dto.Cron).WithIdentity(id).Build();
					var qzJob = JobBuilder.Create<TriggerJob>().WithIdentity(id).WithDescription(spider.Name)
						.RequestRecovery(true)
						.Build();
					await _sched.ScheduleJob(qzJob, trigger);
					transaction.Commit();
				}
				catch (Exception e)
				{
					_logger.LogError($"添加任务失败: {e}");
					try
					{
						transaction.Rollback();
					}
					catch (Exception re)
					{
						_logger.LogError($"回滚添加任务失败: {re}");
					}

					ModelState.AddModelError(string.Empty, "添加任务失败");
					return View("Add", dto);
				}

				return Redirect("/spider");
			}
			else
			{
				return View("Add", dto);
			}
		}

		[HttpGet("spider")]
		public async Task<IActionResult> Retrieve(string q, int page, int size)
		{
			page = page <= 1 ? 1 : page;
			size = size <= 10 ? 10 : size;
			IPagedList<Portal.Entity.Spider> viewModel;
			if (string.IsNullOrWhiteSpace(q))
			{
				viewModel = await _dbContext.Spiders
					.OrderByDescending(x => x.CreationTime)
					.ToPagedListAsync(page, size);
			}
			else
			{
				viewModel = await _dbContext.Spiders.Where(x => x.Name.Contains(q))
					.OrderByDescending(x => x.CreationTime)
					.ToPagedListAsync(page, size);
			}

			return View(viewModel);
		}

		[HttpDelete("spider/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var item = await _dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == id);
			if (item != null)
			{
				_dbContext.Spiders.Remove(item);
				await _dbContext.SaveChangesAsync();
			}

			return Redirect("/");
		}

		[HttpPost("spider/{id}/run")]
		public async Task<IActionResult> Run(int id)
		{
			try
			{
				await JobHelper.RunAsync(_options, _dbContext, id);
				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogError($"启动失败: {e}");
				return StatusCode((int) HttpStatusCode.InternalServerError, new
				{
					e.Message
				});
			}
		}
	}
}