using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Test.Example;
using Java2Dotnet.Spider.Test.Pipeline;
#if NET_CORE
using Java2Dotnet.Spider.JLog;
#endif
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			//var context = spiderBuilder.GetContext();
			//string json = JsonConvert.SerializeObject(context);
			//ScriptSpider spider = new ScriptSpider(context);
			//spider.Run(args);
#if NET_CORE			
            Log.NoConsole = true;
            Log log = new Log("test");
            log.Info("oooooooodata.com",true);
#endif			
            Console.Read();
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
