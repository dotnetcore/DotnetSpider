using System;
using System.Net;
using System.Net.Http;
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
			Console.Read();
		}
	}
}
