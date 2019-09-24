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
			var httpclient = new HttpClient(new SocketsHttpHandler
			{
				Proxy = new WebProxy("http://113.195.17.102:9999"),
				UseProxy = true
			});

			Startup.Execute<EntitySpider>(args);

			// await DistributedSpider.Run();
			Console.Read();
		}
	}
}
