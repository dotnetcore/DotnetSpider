using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.ScriptsConsole
{
	public class Program
	{
		private static string TestUserId;

		public static void Main(string[] args)
		{
            TestUserId=GuidTo16String();
			Core.Spider.PrintInfo();
			string hostName = Dns.GetHostName();
			Console.WriteLine($"HostName: {hostName} UserId: {TestUserId} Time: {DateTime.Now}");
			Console.WriteLine($"Start SpiderNode: {hostName} ...");
			SpiderNode node = new SpiderNode();
			node.Run();
			Console.WriteLine($"Start SpiderNode: {hostName} finished.");

			// Test
			TaskManager manager = new TaskManager();
			manager.AddTestTask(TestUserId, File.ReadAllText("sample.json").Replace("\t","").Replace("\r","").Replace("\n","").Replace("USER_ID", TestUserId));
			manager.TriggerTask(hostName, TestUserId, TaskManager.TestTaskId);

			while (true)
			{
				Thread.Sleep(1000);
			}
		}
        
        public static string GuidTo16String()  
        {  
            long i = 1;  
            foreach (byte b in Guid.NewGuid().ToByteArray())  
                i *= ((int)b + 1);  
            return string.Format("{0:x}", i - DateTime.Now.Ticks);  
        }
	}
}
