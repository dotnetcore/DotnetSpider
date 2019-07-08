using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Portal.Entity;
using Microsoft.EntityFrameworkCore;

namespace DotnetSpider.Portal
{
	public static class JobHelper
	{
		public static async Task RunAsync(PortalOptions options, PortalDbContext dbContext, int jobId)
		{
			var spider = await dbContext.Spiders.FirstOrDefaultAsync(x => x.Id == jobId);
			if (spider == null)
			{
				throw new Exception($"任务 {jobId} 不存在");
			}

			// var docker = new DockerClient.DockerClient(new Uri(options.Docker));
//			bool exists = await docker.ExistsAsync(new
//			{
//				label = new[] {$"dotnetspider.spider.id={spider.Id}"}
//			});
//			if (exists)
//			{
//				throw new Exception($"任务 {spider.Id} 正在运行");
//			}


//			var env = new List<string>((spider.Environment ?? "").Split(new[] {" "},
//				StringSplitOptions.RemoveEmptyEntries))
//			{
//				$"id={spider.Id}",
//				$"type={spider.Type}",
//				$"name={spider.Name}"
//			};
//			var image = $"{spider.Registry}{spider.Repository}:{spider.Tag}";
//			var result = await docker.PullAsync(image);
//			if (!result.Success)
//			{
//				throw new Exception($"接取镜像 {image} 失败: {result.Message}");
//			}
//
//			result = await docker.CreateAsync(image,
//				env.ToArray(),
//				new Dictionary<string, object>
//				{
//					{"dotnetspider.spider.id", spider.Id.ToString()},
//					{"dotnetspider.spider.type", spider.Type},
//					{"dotnetspider.spider.name", spider.Name}
//				}, new[] {options.DockerVolumes});
//			if (!result.Success)
//			{
//				throw new Exception($"创建任务 {jobId} 实例失败: {result.Message}");
//			}
//
//			var spiderContainer = new SpiderContainer
//			{
//				ContainerId = result.Id,
//				SpiderId = spider.Id,
//				Status = "Creating"
//			};
//			result = await docker.StartAsync(spiderContainer.ContainerId);
//			if (result.Success)
//			{
//				spiderContainer.Status = "OK";
//			}
//
//			dbContext.SpiderContainers.Add(spiderContainer);
//			await dbContext.SaveChangesAsync();
		}
	}
}