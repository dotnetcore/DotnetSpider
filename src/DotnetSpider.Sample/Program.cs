using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotnetSpider.Sample.samples;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var spider = Spider.Create<GithubSpider>();
			await spider.RunAsync();
		}
	}
}