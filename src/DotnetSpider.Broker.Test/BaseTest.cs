using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Hubs;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

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

			Services = _service.BuildServiceProvider();
		}
	}
}
