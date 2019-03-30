using System;
using DotnetSpider.Core;
using DotnetSpider.Sample.samples;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			// EntitySpider.Run();

			Startup.Run("-s", "EntitySpider", "-n", "博客园");
			Console.Read();
		}
	}
}