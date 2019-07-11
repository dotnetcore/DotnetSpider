using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotnetSpider.Portal.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DotnetSpider.Portal
{
	public class TriggerJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var jobId = context.JobDetail.Key.Name;
			using (var scope = Common.ServiceProvider.Instance.CreateScope())
			{
				var services = scope.ServiceProvider;
				var logger = services.GetRequiredService<ILogger<TriggerJob>>();
				try
				{
					var options = services.GetRequiredService<PortalOptions>();
					var dbContext = services.GetRequiredService<PortalDbContext>();

					var spider = await dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == int.Parse(jobId));
					if (spider == null)
					{
						logger.LogError($"任务 {jobId} 不存在");
						return;
					}

					DockerClient client = new DockerClientConfiguration(
							new Uri(options.Docker))
						.CreateClient();
					var batch = Guid.NewGuid().ToString("N");
					var env = new List<string>((spider.Environment ?? "").Split(new[] {" "},
						StringSplitOptions.RemoveEmptyEntries))
					{
						$"DOTNET_SPIDER_id={batch}",
						$"DOTNET_SPIDER_type={spider.Type}",
						$"DOTNET_SPIDER_name={spider.Name}"
					};
					var image = $"{spider.Registry}/{spider.Repository}:{spider.Tag}";
					var name = $"dotnetspider-{spider.Id}-{batch}";
					var parameters = new CreateContainerParameters
					{
						Image = image,
						Name = name,
						Labels = new Dictionary<string, string>
						{
							{"dotnetspider.spider.id", spider.Id.ToString()},
							{"dotnetspider.spider.batch", batch},
							{"dotnetspider.spider.type", spider.Type},
							{"dotnetspider.spider.name", spider.Name}
						},
						Env = env,
						HostConfig = new HostConfig()
					};
					parameters.HostConfig.Binds = options.DockerVolumes;
					var result = await client.Containers.CreateContainerAsync(parameters);

					if (result.ID == null)
					{
						logger.LogError($"创建任务 {jobId} 实例失败: {string.Join(", ", result.Warnings)}");
					}

					var spiderContainer = new SpiderContainer
					{
						ContainerId = result.ID,
						Batch = batch,
						SpiderId = spider.Id,
						Status = "Created",
						CreationTime = DateTime.Now
					};

					dbContext.SpiderContainers.Add(spiderContainer);
					await dbContext.SaveChangesAsync();

					var startResult =
						await client.Containers.StartContainerAsync(result.ID, new ContainerStartParameters());
					spiderContainer.Status = startResult ? "Success" : "Failed";

					await dbContext.SaveChangesAsync();

					logger.LogInformation($"触发任务 {jobId} 完成");
				}
				catch (Exception ex)
				{
					logger.LogError($"触发任务 {jobId} 失败: {ex}");
				}
			}
		}
	}
}