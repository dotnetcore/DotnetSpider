using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class ModelSpider
	{
		public static void Run()
		{
			var table = new TableInfo("youku", "show", TableNamePostfix.Today);
			var selector = new Selector("//div[@class='yk-pack pack-film']");
			var fields = new[]
			{
				new FieldSelector(".//img[@class='quic']/@alt","name"),
				new FieldSelector("index", "index",  SelectorType.Enviroment, DataType.Int),
				new FieldSelector("", "id", SelectorType.Enviroment, DataType.Int){ IsPrimary=true},
			};
			var targetRequestSelector = new TargetRequestSelector("//ul[@class='yk-pages']");
			var model = new ModelDefinition(selector, fields, table, targetRequestSelector);
			var json = JsonConvert.SerializeObject(model);
			// Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
			var site = new Site { EncodingName = "UTF-8" };
			for (int i = 1; i < 5; ++i)
			{
				// Add start/feed urls. 添加初始采集链接
				site.AddRequests($"http://list.youku.com/category/show/c_96_s_1_d_1_p_{i}.html");

			}
			Spider spider = Spider.Create(site,
				new QueueDuplicateRemovedScheduler(),
				new ModelProcessor(model))
				.AddPipeline(new ConsoleEntityPipeline());
			spider.Name = "Youku";
			spider.TaskId = "1";
			spider.Run();
		}
	}

	public class ModelSpider2
	{
		class MyDataHandler : IDataHandler
		{
			private readonly List<string> allNames = new List<string> { "cnblogs" };

			public void Handle(ref dynamic data, Page page)
			{
				foreach (var name in allNames)
				{
					if (data["content"].Contains(name))
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
			var table = new TableInfo("websites", "html");
			var fields = new[]
			{
				new FieldSelector(".//title","title"),
				new FieldSelector(Env.UrlPropertyKey, "url",  SelectorType.Enviroment),
				new FieldSelector(".//body", "content" , SelectorType.XPath, DataType.String, int.MaxValue),
				new FieldSelector("is_match", "is_match" , SelectorType.XPath, DataType.Bool),
				new FieldSelector("matchs", "matchs" , SelectorType.XPath, DataType.String, int.MaxValue),
				new FieldSelector("id", "id" , SelectorType.Enviroment, DataType.Int){ IsPrimary=true},
			};
			var targetRequestSelector = new TargetRequestSelector(".", "cnblogs\\.com") { ExcludePatterns = new[] { "\\.png", "\\.jpg", "\\.ico", "\\.gif", "\\.aspx" } };
			var model = new ModelDefinition(null, fields, table, targetRequestSelector);
			var modeProcessor = new ModelProcessor(model);
			modeProcessor.AddDataHanlder(new MyDataHandler());
			var site = new Site { EncodingName = "UTF-8" };
			site.AddRequests($"http://cnblogs.com");
			Spider spider = Spider.Create(site,
				new QueueDuplicateRemovedScheduler(),
				modeProcessor)
				.AddPipeline(new MySqlEntityPipeline());
			spider.Run();
		}
	}
}
