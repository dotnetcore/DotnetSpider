using System;
using DotnetSpider.Sample.samples;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var spider = Spider.Create<GithubSpider>();			
			spider.RunAsync();
			Console.Read();
		}
	}
}