using System;
using System.Collections.Generic;
using System.IO;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.ScriptsConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
			Core.Spider.PrintInfo();

			//Options param = ParseCommand(args);
			//if (param != null)
			//{
			//	StartSpider(param);
			//}
			string json = File.ReadAllText("mysqlsample.json");
			json = Macros.Replace(json);
			SpiderContext spiderContext = JsonConvert.DeserializeObject<SpiderContext>(json);
			List<string> errorMessages;
			if (SpiderContextValidation.Validate(spiderContext, out errorMessages))
			{
				ScriptSpider spider = new ScriptSpider(spiderContext);
				spider.Run(args);
			}
			else
			{
				foreach (var errorMessage in errorMessages)
				{
					Console.WriteLine(errorMessage);
				}
			}
			Console.Read();
		}
    }
}
