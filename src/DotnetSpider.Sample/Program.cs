using System;
using System.Threading.Tasks;
using DotnetSpider.Sample.samples;
using Serilog;
using Serilog.Events;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var	configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("dotnet-spider.log");
			Log.Logger = configure.CreateLogger();
			
			await BaseUsage.Run();

			// await DistributedSpider.Run(); 
			Console.Read();
		}
	}
}