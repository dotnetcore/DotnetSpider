using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotnetSpider.Sample.docs
{
	public class Instance
	{
		public string Cron { get; set; }
		public string Name { get; set; }
		public string[] TargetXpaths { get; set; }
		public string[] TargetPatterns { get; set; }
		public string[] ExcludePatterns { get; set; }
		public string Encording { get; set; }
		public string Url { get; set; }
		public int ThreadNum { get; set; }
		public string Downloader { get; set; }

		public static Instance LoadFrom(string filePath)
		{
			object result = null;

			if (File.Exists(filePath))
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Instance), new XmlRootAttribute("Crawler"));
					result = xmlSerializer.Deserialize(reader);
				}
			}

			return (Instance)result;
		}
	}

	public class ConfigurableSpider
	{
		class MyDataHandler : IDataHandler
		{
			private readonly List<string> allNames = new List<string> { "cnblogs" };

			public void Handle(ref dynamic data, Page page)
			{
				if (string.IsNullOrEmpty(data["content"]))
				{
					page.Bypass = true;
					return;
				}
				foreach (var name in allNames)
				{
					if (data["content"]?.Contains(name))
					{
						data["is_match"] = true;
						if (data["matchs"] == null)
						{
							data["matchs"] = "";
						}
						data["matchs"] += $", {name}";
					}
				}
			}
		}

		public static void Run()
		{
			Instance instance = Instance.LoadFrom("sohu.xml");

			var table = new TableInfo("websites", "html");
			var fields = new[]
			{
				new FieldSelector(".//title","title"),
				new FieldSelector(Env.UrlPropertyKey, "url",  SelectorType.Enviroment),
				new FieldSelector(".//body", "content" , SelectorType.XPath, DataType.String, int.MaxValue),
				new FieldSelector("is_match", "is_match", SelectorType.XPath, DataType.Bool),
				new FieldSelector("matchs", "matchs", SelectorType.XPath, DataType.String, int.MaxValue),
				new FieldSelector("id", "id" , SelectorType.Enviroment, DataType.Int) { IsPrimary = true},
			};
			var targetRequestSelector = new TargetRequestSelector
			{
				XPaths = instance.TargetXpaths,
				Patterns = instance.TargetPatterns,
				ExcludePatterns = instance.ExcludePatterns
			};
			var model = new ModelDefinition(null, fields, table, targetRequestSelector);
			var modeProcessor = new ModelProcessor(model);
			modeProcessor.CleanPound = true;
			modeProcessor.AddDataHanlder(new MyDataHandler());
			var site = new Site { EncodingName = instance.Encording };
			site.AddRequests(instance.Url);
			Spider spider = Spider.Create(site,
				new QueueDuplicateRemovedScheduler(),
				modeProcessor)
				.AddPipeline(new MySqlEntityPipeline());
			if (instance.Downloader.ToLower() == "chrome")
			{
				spider.Downloader = new WebDriverDownloader(Browser.Chrome, new Option { Headless = true });
			}

			spider.Run();
		}
	}
}
