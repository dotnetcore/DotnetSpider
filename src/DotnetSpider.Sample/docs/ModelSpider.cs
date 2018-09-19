//using DotnetSpider.Core;
//using DotnetSpider.Core.Scheduler;
//using DotnetSpider.Extension.Pipeline;
//using DotnetSpider.Extension.Processor;
//using DotnetSpider.Extraction;
//using DotnetSpider.Extraction.Model;
//using DotnetSpider.Extraction.Model.Attribute;
//using Newtonsoft.Json;

//namespace DotnetSpider.Sample.docs
//{
//	public class ModelSpider
//	{
//		public static void Run()
//		{
//			var table = new Table("youku", "show", TableNamePostfix.Today);
//			var selector = new Selector("//div[@class='yk-pack pack-film']");
//			var fields = new[]
//			{
//				new Field(".//img[@class='quic']/@alt","name"),
//				new Field("index", "index",  SelectorType.Enviroment, DataType.Int),
//				new Field("", "id", SelectorType.Enviroment, DataType.Int){ IsPrimary=true},
//			};
//			var targetRequestSelector = new Target("//ul[@class='yk-pages']");
//			var model = new ModelDefinition(selector, fields, table, targetRequestSelector);
//			var json = JsonConvert.SerializeObject(model);

//			Spider spider = Spider.Create(
//				new QueueDuplicateRemovedScheduler(),
//				new ModelProcessor(model))
//				.AddPipeline(new ConsoleEntityPipeline());
//			spider.Name = "Youku";
//			spider.EncodingName = "UTF-8";
//			spider.TaskId = "1";
//			for (int i = 1; i < 5; ++i)
//			{
//				// Add start/feed urls. 添加初始采集链接
//				spider.AddRequests($"http://list.youku.com/category/show/c_96_s_1_d_1_p_{i}.html");

//			}

//			spider.Run();
//		}
//	}
//}
