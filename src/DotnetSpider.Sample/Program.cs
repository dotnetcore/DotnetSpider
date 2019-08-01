using System;
using System.Collections.Generic;
using DotnetSpider.Downloader;
using DotnetSpider.Sample.samples;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
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


			var list = new List<string>() {"20190725144127", "20190801110137", "20190801165949"};
			list.Sort(new StringCompare());
			Startup.Execute<EntitySpider>(args);

			// await DistributedSpider.Run();
			Console.Read();
		}

		class StringCompare : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				var result = String.CompareOrdinal(x, y);
				if (result == 0)
				{
					return 0;
				}
				else if (result > 0)
				{
					return 0;
				}
				else
				{
					return 1;
				}
			}
		}
	}
}
