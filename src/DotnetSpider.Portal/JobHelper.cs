using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
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

			DockerClient client = new DockerClientConfiguration(
					new Uri("http://localhost:2376"))
				.CreateClient();
			var env = new List<string>((spider.Environment ?? "").Split(new[] {" "},
				StringSplitOptions.RemoveEmptyEntries))
			{
				$"id={spider.Id}",
				$"type={spider.Type}",
				$"name={spider.Name}"
			};
			var image = $"{spider.Registry}{spider.Repository}:{spider.Tag}";

			var result = await client.Containers.CreateContainerAsync(new CreateContainerParameters
			{
				Image = image,
				Name = $"dotnetspider-{spider.Id}",
				Labels = new Dictionary<string, string>
				{
					{"dotnetspider.spider.id", spider.Id.ToString()},
					{"dotnetspider.spider.type", spider.Type},
					{"dotnetspider.spider.name", spider.Name}
				},
				
				Volumes = new Dictionary<string, EmptyStruct>
				{
					{
						options.DockerVolumes, new EmptyStruct()
					}
				},
				Env = env
			});


			if (result.ID == null)
			{
				throw new Exception($"创建任务 {jobId} 实例失败: {string.Join(", ", result.Warnings)}");
			}

			var spiderContainer = new SpiderContainer
			{
				ContainerId = result.ID,
				SpiderId = spider.Id,
				Status = "OK"
			};

			dbContext.SpiderContainers.Add(spiderContainer);
			await dbContext.SaveChangesAsync();
		}
	}
}