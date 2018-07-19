using System.IO;
using DotnetSpider.Common;
using Xunit;

namespace DotnetSpider.Core.Test
{

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

		[Fact(DisplayName = "JsonFilePipeline")]
		public void Process()
		{
			Core.Pipeline.JsonFilePipeline pipeline = new Core.Pipeline.JsonFilePipeline();
			ISpider spider = new DefaultSpider("test", new Site());

			var folder = pipeline.GetDataFolder(spider);
			if (Directory.Exists(folder))
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					try
					{
						File.Delete(file);
					}
					catch { }
				}
			}
			pipeline.Process(new[] { _resultItems }, spider.Logger, spider);
			pipeline.Dispose();
			string dataFile = Directory.GetFiles(folder)[0];
			string content = File.ReadAllText(dataFile);
			string expected = $"{{\"content\":\"爬虫工具\"}}{System.Environment.NewLine}";
			Assert.Equal(expected, content);
		}
	}
}
