using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using DotnetSpider.Broker.Services;

namespace DotnetSpider.Broker.Test
{
	public class BaseTest
	{
		private readonly IServiceCollection _service = new ServiceCollection();

		protected IServiceProvider Services;

		public BaseTest()
		{
			var configurationFile = "appsettings.json";
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(configurationFile, optional: true)
				.Build();

			_service.AddDbContext<BrokerDbContext>(ops =>
			ops.UseSqlServer(config.GetConnectionString("DefaultConnection")));
			_service.AddScoped<WorkerHub>();

			_service.AddScoped<IWorkerService, WorkerService>();
			_service.AddScoped<INodeService, NodeService>();
			_service.AddScoped<INodeStatusService, NodeStatusService>();
			Services = _service.BuildServiceProvider();
		}
	}
}
