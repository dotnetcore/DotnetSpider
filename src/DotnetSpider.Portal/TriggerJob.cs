using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Portal.Core;
using DotnetSpider.Portal.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog.Core;
using LogLevel = Quartz.Logging.LogLevel;

namespace DotnetSpider.Portal
{
	public class TriggerJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var jobId = context.JobDetail.Key.Name;
			var logger = Ioc.ServiceProvider.GetRequiredService<ILogger<TriggerJob>>();
			try
			{
				var options = Ioc.ServiceProvider.GetRequiredService<PortalOptions>();
				var dbContext = Ioc.ServiceProvider.GetRequiredService<PortalDbContext>();
				var spider = await dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == int.Parse(jobId));
				if (spider == null)
				{
					logger.LogError($"任务 {jobId} 不存在");
					return;
				}

				var docker = new DockerClient.DockerClient(new Uri(options.Docker));
				if (spider.Single)
				{
					bool exists = await docker.ExistsAsync(new
					{
						Label = new[] {$"dotnetspider.spider.id={spider.Id}"}
					});
					if (exists)
					{
						logger.LogError($"任务 {spider.Id} 正在运行");
						return;
					}
				}

				var result = await docker.CreateAsync(spider.Image,
					spider.Arguments.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries),
					new Dictionary<string, object>
					{
						{"dotnetspider.spider.id", spider.Id},
						{"dotnetspider.spider.class", spider.Class},
						{"dotnetspider.spider.name", spider.Name}
					},new []{options.DockerVolumes});
				if (!result.Success)
				{
					logger.LogError($"创建任务 {jobId} 实例失败: {result.Message}");
				}

				var spiderContainer = new SpiderContainer
				{
					ContainerId = result.Id,
					SpiderId = spider.Id,
					Status = "created"
				};
				result = await docker.StartAsync(spiderContainer.ContainerId);
				if (result.Success)
				{
					spiderContainer.Status = "started";
				}

				dbContext.SpiderContainers.Add(spiderContainer);
				await dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				logger.LogError($"触发任务 {jobId} 失败: {ex}");
			}
		}
	}
}