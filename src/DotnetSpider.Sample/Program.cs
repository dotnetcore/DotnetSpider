using System;
using System.Threading.Tasks;
using DotnetSpider.Sample.samples;
using Pamirs.Spiders.Speiyou;
using Serilog;
using Serilog.Events;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configure = new LoggerConfiguration()
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

			Environment.SetEnvironmentVariable("DOTNET_SPIDER_ID", "36f51567-7b44-4dc3-936b-1a1e077bb608");
			Environment.SetEnvironmentVariable("DOTNET_SPIDER_NAME", "test");
			Environment.SetEnvironmentVariable("DOTNET_SPIDER_TYPE", "Pamirs.Spiders.Speiyou.AllMenuSpider");
			Environment.SetEnvironmentVariable("DOTNET_SPIDER_SPEIYOU_TOKEN",
				"eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJmZmZmZmZmZi1iNjhhLWRkYzQtMmE0Zi1lZTVjM2U1YWY4YjcifQ.FcIAA20CQGJ_K_b67tOx-P3ADDjfBGwdCS-tT7YMbcbSkO3e9tZNVgNWa0QsdTU-zihjbI_F7pH3IviJ-54sYw");

			Startup.Execute<CourseSpider>();
			// await BaseUsage.Run();

			// await DistributedSpider.Run(); 
			Console.Read();
		}
	}
}