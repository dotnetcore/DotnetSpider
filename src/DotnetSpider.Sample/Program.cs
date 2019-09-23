using System;
using DotnetSpider.Sample.samples;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{



			Startup.Execute<EntitySpider>(args);

			// await DistributedSpider.Run();
			Console.Read();
		}
	}
}
