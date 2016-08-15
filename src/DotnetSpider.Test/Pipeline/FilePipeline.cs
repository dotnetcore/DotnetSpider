using System.IO;
using DotnetSpider.Core;
using Xunit;

namespace DotnetSpider.Test.Pipeline
{
	public class FilePipeline
	{
		private Core.ResultItems _resultItems;

		public FilePipeline()
		{
			Before();
		}

		private void Before()
		{
			_resultItems = new Core.ResultItems();
			_resultItems.AddOrUpdateResultItem("content", "爬虫工具");
			Request request = new Request("http://www.baidu.com", 1, null);
			_resultItems.Request = request;
		}

		[Fact]
		public void Process()
		{
			Core.Pipeline.FilePipeline filePipeline = new Core.Pipeline.FilePipeline();
			ISpider spider = new DefaultSpider("test", new Core.Site());
			filePipeline.InitPipeline(spider);
			var folder = filePipeline.GetDataForlder();
			if (Directory.Exists(folder))
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					File.Delete(file);
				}
			}
			filePipeline.Process(_resultItems);
			string dataFile = Directory.GetFiles(folder)[0];
			string content = File.ReadAllText(dataFile);
			string expected = "url:\thttp://www.baidu.com/\r\ncontent:\t爬虫工具\r\n";
			Assert.Equal(expected, content);
		}
	}
}
