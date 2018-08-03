using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Broker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var configurationFile = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ?
													"appsettings.Development.json" : "appsettings.json";

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.WriteTo.RollingFile(Path.Combine(Directory.GetCurrentDirectory(), "{Date}.log"))
				.WriteTo.Console()
				.CreateLogger();

			Log.Information("Welcome to DotnetSpider.Broker!");

			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(configurationFile, optional: true)
				.Build();

			var host = WebHost.CreateDefaultBuilder(args).UseConfiguration(config)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>().UseSerilog().Build();
			host.Run();
		}

	}
}
