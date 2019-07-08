using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Portal
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			DockerClient client = new DockerClientConfiguration(
					new Uri("http://localhost:4243"))
				.CreateClient();
			var a = await client.Containers.ListContainersAsync(new ContainersListParameters());

			// docker run --name {id} --label  {image} {arguments}

			var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("dotnet-spider-portal.log");

			Log.Logger = loggerConfiguration.CreateLogger();

			await CreateWebHostBuilder(args).Build().RunAsync();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>().UseSerilog().UseUrls("http://0.0.0.0:7896");
	}
}