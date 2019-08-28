using System;

namespace DotnetSpider.Spiders.Startup
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") == "Development")
			{
				Environment.SetEnvironmentVariable("DOTNET_SPIDER_ID", "36f51567-7b44-4dc3-936b-1a1e077bb608");
				Environment.SetEnvironmentVariable("DOTNET_SPIDER_NAME", "cnblogs");
				Environment.SetEnvironmentVariable("DOTNET_SPIDER_TYPE", "DotnetSpider.Spiders.CnblogsSpider");
				Environment.SetEnvironmentVariable("DOTNET_SPIDER_PRINT_SPIDERS", "true");
				Environment.SetEnvironmentVariable("DOTNET_SPIDER_DISTRIBUTED", "true");
			}

			var startup = new MyStartup();
			startup.Execute();
			Console.WriteLine("Bye");
		}
	}
}
