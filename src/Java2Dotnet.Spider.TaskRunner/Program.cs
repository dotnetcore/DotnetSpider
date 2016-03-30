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
		private readonly static string TestUserId = Guid.NewGuid().ToString();

		public static void Main(string[] args)
		{
			Core.Spider.PrintInfo();
			string hostName = Dns.GetHostName();
			Console.WriteLine($"HostName: {hostName} UserId: {TestUserId} Time: {DateTime.Now}");
			Console.WriteLine($"Start SpiderNode: {hostName} ...");
			SpiderNode node = new SpiderNode();
			node.Run();
			Console.WriteLine($"Start SpiderNode: {hostName} finished.");

			// Test
			TaskManager manager = new TaskManager();
			manager.AddTestTask(TestUserId, File.ReadAllText("sample.json").Replace("USER_ID", TestUserId));
			manager.TriggerTask(hostName, TestUserId, TaskManager.TestTaskId);

			while (true)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
