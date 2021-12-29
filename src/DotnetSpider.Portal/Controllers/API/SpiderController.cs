using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DotnetSpider.Extensions;
using DotnetSpider.Infrastructure;
using DotnetSpider.MessageQueue;
using DotnetSpider.Portal.BackgroundService;
using DotnetSpider.Portal.Common;
using DotnetSpider.Portal.Data;
using DotnetSpider.Portal.ViewObject;
using DotnetSpider.Statistics.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DotnetSpider.Portal.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/spiders")]
	public class SpiderController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;
		private readonly IScheduler _sched;
		private readonly IMessageQueue _mq;
		private readonly IMapper _mapper;

		public SpiderController(PortalDbContext dbContext,
			IScheduler sched,
			ILogger<SpiderController> logger, IMessageQueue mq, IMapper mapper)
		{
			_logger = logger;
			_mq = mq;
			_mapper = mapper;
			_dbContext = dbContext;
			_sched = sched;
		}

		[HttpPost]
		public async Task<bool> CreateAsync(SpiderViewObject vo)
		{
			if (ModelState.IsValid)
			{
				var exists = await _dbContext.Spiders.AnyAsync(x =>
					x.Name == vo.Name);
				if (exists)
				{
					throw new ApplicationException($"Name {vo.Name} exists");
				}

				try
				{
					TriggerBuilder.Create().WithCronSchedule(vo.Cron).Build();
				}
				catch
				{
					throw new ApplicationException($"Cron {vo.Cron} is invalid");
				}

				var spider = _mapper.Map<Data.Spider>(vo);
				spider.Enabled = true;
				spider.CreationTime = DateTimeOffset.Now;
				spider.LastModificationTime = DateTimeOffset.Now;

				await _dbContext.Spiders.AddAsync(spider);
				await _dbContext.SaveChangesAsync();

				var id = spider.Id.ToString();
				await ScheduleJobAsync(vo.Cron, id, spider.Name);
				return true;
			}
			else
			{
				throw new ApplicationException("ModelState is invalid");
			}
		}

		[HttpPut("{id}")]
		public async Task<bool> UpdateAsync(int id, [FromBody] SpiderViewObject vo)
		{
			if (ModelState.IsValid)
			{
				var spider = await _dbContext.Spiders.FirstOrDefaultAsync(x =>
					x.Id == id);
				if (spider == null)
				{
					throw new ApplicationException($"Spider {id} exists");
				}

				try
				{
					TriggerBuilder.Create().WithCronSchedule(vo.Cron).Build();
				}
				catch
				{
					throw new ApplicationException($"Cron {vo.Cron} is invalid");
				}

				var reSched = spider.Cron != vo.Cron;

				spider = _mapper.Map(vo, spider);
				spider.LastModificationTime = DateTimeOffset.Now;

				_dbContext.Spiders.Update(spider);
				await _dbContext.SaveChangesAsync();

				if (reSched)
				{
					var jobId = id.ToString();
					var deleted = await _sched.DeleteJob(new JobKey(jobId));
					if (!deleted)
					{
						throw new ApplicationException("Delete quartz job failed");
					}

					await ScheduleJobAsync(spider.Cron, jobId, spider.Name);
				}

				return true;
			}
			else
			{
				throw new ApplicationException("ModelState is invalid");
			}
		}

		[HttpGet]
		public async Task<PagedResult<ListSpiderViewObject>> PagedQueryAsync(string keyword, int page, int limit)
		{
			PagedResult<Data.Spider> @out;
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				@out = await _dbContext
					.Set<Data.Spider>()
					.PagedQueryAsync(page, limit, x => x.Name.Contains(keyword),
						new OrderCondition<Data.Spider, DateTimeOffset>(x => x.LastModificationTime));
			}
			else
			{
				@out = await _dbContext
					.Set<Data.Spider>()
					.PagedQueryAsync(page, limit, null,
						new OrderCondition<Data.Spider, DateTimeOffset>(x => x.LastModificationTime));
			}

			return _mapper.ToPagedQueryResult<Data.Spider, ListSpiderViewObject>(@out);
		}

		[HttpPut("{id}/run")]
		public async Task<bool> RunAsync(int id)
		{
			try
			{
				await _sched.TriggerJob(new JobKey(id.ToString()));
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError($"启动失败: {e}");
				return false;
			}
		}

		[HttpDelete("{id}")]
		public async Task<bool> DeleteAsync(int id)
		{
			var item = await _dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == id);
			if (item != null)
			{
				_dbContext.Spiders.Remove(item);
				await _dbContext.SaveChangesAsync();
			}

			return true;
		}

		[HttpPut("{id}/disable")]
		public async Task<bool> DisableAsync(int id)
		{
			try
			{
				var item = await _dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == id);
				if (item != null && item.Enabled)
				{
					item.Enabled = false;
					_dbContext.Spiders.Update(item);
					await _dbContext.SaveChangesAsync();
				}

				return true;
			}
			catch (Exception e)
			{
				_logger.LogError($"禁用失败: {e}");
				return false;
			}
		}

		[HttpPut("{id}/enable")]
		public async Task<bool> EnableAsync(int id)
		{
			try
			{
				var item = await _dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == id);
				if (item != null && !item.Enabled)
				{
					item.Enabled = true;
					_dbContext.Spiders.Update(item);
					await _dbContext.SaveChangesAsync();
				}

				return true;
			}
			catch (Exception e)
			{
				_logger.LogError($"启用失败: {e}");
				return false;
			}
		}

		[HttpGet("{id}/histories")]
		public async Task<PagedResult<SpiderHistoryViewObject>> PagedQueryHistoryAsync(int id, int page, int limit)
		{
			page = page <= 1 ? 1 : page;
			limit = limit <= 15 ? 15 : limit;

			var pagedQueryResult = await _dbContext.SpiderHistories.PagedQueryAsync(page, limit, x => x.SpiderId == id,
				new OrderCondition<SpiderHistory, int>(x => x.Id));
			var @out = _mapper.ToPagedQueryResult<SpiderHistory, SpiderHistoryViewObject>(pagedQueryResult);
			var batches = @out.Data.Select(x => x.Batch);
			var dict = await _dbContext.Set<SpiderStatistics>().Where(x => batches.Contains(x.Id))
				.ToDictionaryAsync(x => x.Id, x => x);
			foreach (var item in @out.Data)
			{
				if (!dict.ContainsKey(item.Batch))
				{
					continue;
				}

				var statistics = dict[item.Batch];
				if (statistics != null)
				{
					item.Total = statistics.Total;
					item.Failure = statistics.Failure;
					item.Success = statistics.Success;
					item.Start = statistics.Start?.ToString("yyyy-MM-dd HH:mm:ss");
					item.Exit = statistics.Exit?.ToString("yyyy-MM-dd HH:mm:ss");
					item.Left = item.Total - item.Success;
				}
			}

			return @out;
		}

		[HttpPut("{id}/exit")]
		public async Task<bool> ExitAsync(int id)
		{
			var spiderHistory = await _dbContext.SpiderHistories.FirstOrDefaultAsync(x => x.Id == id);
			if (spiderHistory == null)
			{
				throw new ApplicationException("Spider history is not exits");
			}

			var spiderId = spiderHistory.Batch;
			var topic = string.Format(Topics.Spider, spiderHistory.Batch);
			_logger.LogInformation($"Try stop spider {topic}");
			await _mq.PublishAsBytesAsync(topic,
				new Messages.Spider.Exit {SpiderId = spiderId});
			return true;
		}

		private async Task ScheduleJobAsync(string cron, string id, string name)
		{
			var trigger = TriggerBuilder.Create().WithCronSchedule(cron).WithIdentity(id)
				.Build();
			var qzJob = JobBuilder.Create<QuartzJob>().WithIdentity(id).WithDescription(name)
				.RequestRecovery(true).Build();
			await _sched.ScheduleJob(qzJob, trigger);
		}
	}
}
