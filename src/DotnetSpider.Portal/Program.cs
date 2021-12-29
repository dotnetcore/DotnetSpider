using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Portal
{
	public class Program
	{
		public static IWebHostBuilder Builder;

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

			Builder = WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseSerilog()
#if DEBUG
				.UseUrls("http://localhost:7896");
#else
				.UseUrls("http://+:7896");
#endif
			await Builder.Build().RunAsync();
		}
	}
}
