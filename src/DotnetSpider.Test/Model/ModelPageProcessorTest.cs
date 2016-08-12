//using DotnetSpider.Core;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace DotnetSpider.Test.Model
//{
//	
//	public class ModelPageProcessorTest
//	{
//		[TargetUrl(new[] { "http://codecraft.us/foo" })]
//		public class ModelFoo
//		{
//			[PropertyExtractBy(Expression = "//div/@foo", NotNull = true)]
//			public string Foo { get; set; }
//		}

//		[TargetUrl(new[] { "." })]
//		public class ModelBar
//		{
//			[PropertyExtractBy(Expression = "//div[2]/@bar", NotNull = true)]
//			public string Bar { get; set; }
//		}

//		[Fact]
//		public void testMultiModel_should_not_skip_when_match()
//		{
//			Page page = new Page(new Request("http://codecraft.us/foo", 1, null), ContentType.Html);
//			page.Content = "<div foo='foo'></div><div bar='bar'></div>";
//			page.Url = ("http://codecraft.us/foo");
//			EntityProcessor modelPageProcessor = new EntityProcessor(new Site());
//			JObject entity1 = JsonConvert.DeserializeObject(BaseTask.ConvertToJson(typeof(ModelFoo))) as JObject;
//			JObject entity2 = JsonConvert.DeserializeObject(BaseTask.ConvertToJson(typeof(ModelBar))) as JObject;
//			modelPageProcessor.AddEntity(entity1);
//			modelPageProcessor.AddEntity(entity2);
//			modelPageProcessor.Process(page);

//			string result1 = page.ResultItems.GetResultItem(typeof(ModelFoo).FullName).ToString().Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "");
//			string result2 = page.ResultItems.GetResultItem(typeof(ModelBar).FullName).ToString().Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "");
//			Assert.Equal("{\"Foo\":\"foo\"}", result1);
//			Assert.Equal("{\"Bar\":\"bar\"}", result2);
//		}
//	}
//}
