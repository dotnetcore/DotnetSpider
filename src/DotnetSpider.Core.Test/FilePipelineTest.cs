using System.IO;
using DotnetSpider.Common;
using Xunit;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Test
{

	public class FilePipelineTest
	{
		private ResultItems _resultItems;

		public FilePipelineTest()
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

		[Fact(DisplayName = "FilePipeline")]
		public void Process()
		{
			Pipeline.FilePipeline filePipeline = new Pipeline.FilePipeline();
			ISpider spider = new DefaultSpider("test");

			var folder = filePipeline.GetDataFolder(spider);
			if (Directory.Exists(folder))
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					File.Delete(file);
				}
			}
			filePipeline.Process(new[] { _resultItems }, spider);
			string dataFile = Directory.GetFiles(folder)[0];
			string content = File.ReadAllText(dataFile);
			string expected = $"url:\thttp://www.baidu.com{System.Environment.NewLine}content:\t爬虫工具{System.Environment.NewLine}";
			Assert.Equal(expected, content);
		}
	}
}
