using System;
using Serilog;

namespace DotnetSpider.Spiders
{
	public class Program
	{
		static void Main(string[] args)
		{
			var startup = new MyStartup();
			startup.Execute();
		}
	}
}