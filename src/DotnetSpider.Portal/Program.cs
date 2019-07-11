using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Portal
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("/logs/portal.log");

			Log.Logger = loggerConfiguration.CreateLogger();

			var builder = WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(x =>
				{
					if (File.Exists("appsettings.json"))
					{
						x.AddJsonFile("appsettings.json");
					}
					x.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
					x.AddCommandLine(args);
				})
				.UseStartup<Startup>().UseSerilog().UseUrls("http://0.0.0.0:7896");
			await builder.Build().RunAsync();
		}
	}
}