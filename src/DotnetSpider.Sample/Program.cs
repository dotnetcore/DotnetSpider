using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotnetSpider.Core;
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