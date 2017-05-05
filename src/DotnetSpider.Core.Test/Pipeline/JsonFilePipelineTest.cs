using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetSpider.Core.Test.Pipeline
{
	[TestClass]
	public class JsonFilePipelineTest
	{
		private ResultItems _resultItems;

		public JsonFilePipelineTest()
		{
			Before();
		}

		private void Before()
		{
			_resultItems = new ResultItems();
			_resultItems.AddOrUpdateResultItem("content", "爬虫工具");
			Request request = new Request("http://www.baidu.com", null);
			_resultItems.Request = request;
		}

		[TestMethod]
		public void Process()
		{
			Core.Pipeline.JsonFilePipeline pipeline = new Core.Pipeline.JsonFilePipeline();
			ISpider spider = new DefaultSpider("test", new Site());
			pipeline.InitPipeline(spider);
			var folder = pipeline.GetDataForlder();
			if (Directory.Exists(folder))
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					File.Delete(file);
				}
			}
			pipeline.Process(_resultItems);
			string dataFile = Directory.GetFiles(folder)[0];
			string content = File.ReadAllText(dataFile);
			string expected = "{\"content\":\"爬虫工具\"}\r\n";
			Assert.AreEqual(expected, content);
		}
	}
}
