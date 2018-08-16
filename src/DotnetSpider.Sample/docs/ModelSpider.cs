using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Downloader;
using DotnetSpider.Extension.Downloader;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

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
}
