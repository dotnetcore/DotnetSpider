using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace DotnetSpider.Portal
{
	public class CleanDockerContainerService : BackgroundService
	{
		private readonly PortalOptions _options;
		private readonly ILogger _logger;

		public CleanDockerContainerService(PortalOptions options, ILogger<CleanDockerContainerService> logger)
		{
			_options = options;
			_logger = logger;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.Factory.StartNew(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					DockerClient client = new DockerClientConfiguration(
							new Uri(_options.Docker))
						.CreateClient();

					try
					{
						var containers = await client.Containers.ListContainersAsync(new ContainersListParameters
						{
							Filters = new Dictionary<string, IDictionary<string, bool>>
							{
								{"status", new Dictionary<string, bool> {{"exited", true}}},
								{"label", new Dictionary<string, bool> {{"dotnetspider.spider.id", true}}},
							}
						}, stoppingToken);
						foreach (var container in containers)
						{
							await client.Containers.RemoveContainerAsync(container.ID,
								new ContainerRemoveParameters(),
								stoppingToken);

							_logger.LogInformation($"删除过期实例: {JsonConvert.SerializeObject(container.Labels)}");
						}
					}
					catch (Exception e)
					{
						_logger.LogError(e.ToString());
					}

					Thread.Sleep(15000);
				}
			}, stoppingToken);
		}
	}
}