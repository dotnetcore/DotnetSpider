using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using Xunit;

namespace DotnetSpider.Test.Pipeline
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
			Request request = new Request("http://www.baidu.com", 1, null);
			_resultItems.Request = request;
		}

		[Fact]
		public void TestProcess()
		{
			FilePipeline filePipeline = new FilePipeline();
			filePipeline.Process(_resultItems);
		}
	}
}
