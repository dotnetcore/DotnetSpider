using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Portal.Entity;
using DotnetSpider.Portal.Models.Spider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class SpiderController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly IScheduler _sched;
		private readonly PortalOptions _options;
		private static HttpClient _httpClient;

		public SpiderController(PortalDbContext dbContext,
			PortalOptions options,
			IScheduler sched,
			ILogger<SpiderController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
			_sched = sched;
			_options = options;
		}

		[HttpGet("spider/add")]
		public async Task<IActionResult> Add(string repository)
		{
			var viewModel = new AddSpiderViewModel();
			var dockerRepository = await _dbContext.DockerRepositories.FirstAsync(x =>
				$"{x.Registry}{x.Repository}".Replace("http://", "").Replace("https://", "") == repository);

			var httpClient = Common.HttpClientFactory.GetHttpClient(dockerRepository.Registry,
				dockerRepository.UserName, dockerRepository.Password);
			var json = await httpClient.GetStringAsync(
				$"{dockerRepository.Registry}v2/{dockerRepository.Repository}/tags/list");
			var repositoryTags = JsonConvert.DeserializeObject<RepositoryTags>(json);
			viewModel.Tags = repositoryTags.Tags;
			return View(viewModel);
		}


		[HttpPost("spider/add")]
		public async Task<IActionResult> Add(AddSpiderViewModel dto)
		{
			if (!ModelState.IsValid)
			{
				return await Add(dto.Repository);
			}

			var exists = await _dbContext.Spiders.AnyAsync(x =>
				x.Name == dto.Name);
			if (exists)
			{
				ModelState.AddModelError("Name", "名称已经存在");
			}

			var dockerRepository = await _dbContext.DockerRepositories.FirstAsync(x =>
				$"{x.Registry}{x.Repository}".Replace("http://", "").Replace("https://", "") == dto.Repository);
			if (dockerRepository == null)
			{
				ModelState.AddModelError("Repository", "镜像仓库不存在");
				return await Add(dto.Repository);
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
				// var transaction = await _dbContext.Database.BeginTransactionAsync();
				try
				{
					var spider = new Portal.Entity.Spider
					{
						Name = dto.Name,
						Cron = dto.Cron,
						Type = dto.Type,
						Registry = dockerRepository.Registry,
						Repository = dto.Repository,
						Environment = dto.Environment,
						Tag = dto.Tag,
						Arguments = dto.Arguments,
						CreationTime = DateTime.Now
					};
					_dbContext.Spiders.Add(spider);
					await _dbContext.SaveChangesAsync();

					var id = spider.Id.ToString();
					var trigger = TriggerBuilder.Create().WithCronSchedule(dto.Cron).WithIdentity(id).Build();
					var qzJob = JobBuilder.Create<TriggerJob>().WithIdentity(id).WithDescription(spider.Name)
						.RequestRecovery(true)
						.Build();
					await _sched.ScheduleJob(qzJob, trigger);
					//transaction.Commit();
				}
				catch (Exception e)
				{
					_logger.LogError($"添加任务失败: {e}");
//					try
//					{
//						transaction.Rollback();
//					}
//					catch (Exception re)
//					{
//						_logger.LogError($"回滚添加任务失败: {re}");
//					}

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
				await _sched.TriggerJob(new JobKey(id.ToString()));
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