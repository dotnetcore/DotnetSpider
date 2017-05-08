using DotnetSpider.Extension.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetSpider.Sample
{
	public class ConfigurableSpider
	{
		public static void Run()
		{
			string json = File.ReadAllText("jdCategory.json");
			var spider = JsonConvert.DeserializeObject<JsonSpiderContext>(json);
			spider.Run();
			Console.Read();
		}
	}
}
