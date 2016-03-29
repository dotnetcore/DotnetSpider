using System;
using System.Collections.Generic;
using System.IO;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
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
			string json = File.ReadAllText(args[0]);
			json = Macros.Replace(json);
			JsonSpiderContext spiderContext = JsonConvert.DeserializeObject<JsonSpiderContext>(json);
			List<string> errorMessages;
			if (SpiderContextValidation.Validate(spiderContext, out errorMessages))
			{
				ScriptSpider spider = new ScriptSpider(spiderContext.ToRuntimeContext());
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
