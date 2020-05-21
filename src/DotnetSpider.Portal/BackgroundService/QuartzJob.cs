using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotnetSpider.Portal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using ServiceProvider = DotnetSpider.Portal.Common.ServiceProvider;

namespace DotnetSpider.Portal.BackgroundService
{
	public class QuartzJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var jobId = context.JobDetail.Key.Name;

			using (var scope = ServiceProvider.Instance.CreateScope())
			{
				var services = scope.ServiceProvider;
				var logger = services.GetRequiredService<ILogger<QuartzJob>>();
				logger.LogInformation($"触发任务 {jobId}");
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

					if (!spider.Enabled)
					{
						logger.LogError($"任务 {jobId} 被禁用");
						return;
					}

					var client = new DockerClientConfiguration(
							new Uri(options.Docker))
						.CreateClient();
					var batch = Guid.NewGuid().ToString("N");
					var env = new List<string>((spider.Environment ?? "").Split(new[] {" ", "\n"},
						StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
					{
						$"DOTNET_SPIDER_ID={batch}",
						$"DOTNET_SPIDER_NAME={spider.Name}"
					};

					var name = $"dotnetspider-{spider.Id}-{batch}";
					var parameters = new CreateContainerParameters
					{
						Image = spider.Image,
						Name = name,
						Labels = new Dictionary<string, string>
						{
							{"dotnetspider.spider.id", spider.Id.ToString()},
							{"dotnetspider.spider.batch", batch},
							{"dotnetspider.spider.name", spider.Name}
						},
						Env = env,
						HostConfig = new HostConfig()
					};
					var volumes = new HashSet<string>();
					foreach (var volume in options.DockerVolumes)
					{
						volumes.Add(volume);
					}

					var configVolumes = new List<string>((spider.Volume ?? "").Split(new[] {" ", "\n"},
						StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
					foreach (var volume in configVolumes)
					{
						volumes.Add(volume);
					}

					parameters.HostConfig.Binds = volumes.ToList();

					var result = await client.Containers.CreateContainerAsync(parameters);

					if (result.ID == null)
					{
						logger.LogError($"创建任务 {jobId} 实例失败: {string.Join(", ", result.Warnings)}");
					}

					var spiderContainer = new SpiderHistory
					{
						ContainerId = result.ID,
						Batch = batch,
						SpiderId = spider.Id,
						SpiderName = spider.Name,
						Status = "Created",
						CreationTime = DateTimeOffset.Now
					};

					dbContext.SpiderHistories.Add(spiderContainer);
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
