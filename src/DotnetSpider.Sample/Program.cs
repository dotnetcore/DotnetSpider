using System;
using System.Threading.Tasks;
using DotnetSpider.Sample.samples;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// await BaseUsage.Run();

			await DistributedSpider.Run(); 
			Console.Read();
		}

		static Task Write(string msg)
		{
			Console.WriteLine(msg);
			return Task.CompletedTask;
		}
	}
}