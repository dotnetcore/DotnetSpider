using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Test.Example;
using Java2Dotnet.Spider.Test.Pipeline;

namespace Java2Dotnet.Spider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			ScriptSpider spider = new ScriptSpider(spiderBuilder.GetContext());
			spider.Run(args);
			//Run(() =>
			//{
			//	FilePipelineTest test1 = new FilePipelineTest();
			//	test1.Before();
			//	test1.TestProcess();
			//}, "FilePipelineTest", "TestProcess");


			//Run(() =>
			//{
			//	ResultItemsCollectorsPipelineTest test = new ResultItemsCollectorsPipelineTest();
			//	test.TestCollectorPipeline();
			//}, "ResultItemsCollectorsPipelineTest", "TestCollectorPipeline");


			//Run(() =>
			//{
			//	SpiderTest test = new SpiderTest();
			//	test.TestStartAndStop();
			//}, "SpiderTest", "TestStartAndStop");
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("Hello World!");
		}

		static void Run(Action a, string type, string name)
		{
			try
			{
				a();
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"[{type}] [{name}] ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("PASS.");
				Console.WriteLine();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"[{type}] [{name}] ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("FAILD.");
				Console.WriteLine();
			}
		}
	}
}
