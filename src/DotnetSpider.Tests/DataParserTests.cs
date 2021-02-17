using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Http;
using DotnetSpider.Selector;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotnetSpider.Tests
{
	public partial class DataParserTests : TestBase
	{
		/// <summary>
		/// 从 HTML 指定的 XPATH 元素下面查找所有的 URL
		/// </summary>
		[Fact]
		public async Task XpathFollow()
		{
			var request = new Request("http://cnblogs.com");
			var dataContext =
				new DataFlowContext(null, new SpiderOptions(), request,
					new Response {Content = new ByteArrayContent(File.ReadAllBytes("cnblogs.html"))});


			var dataParser = new TestDataParser();
			dataParser.AddFollowRequestQuerier(Selectors.XPath(".//div[@class='pager']"));

			await dataParser.HandleAsync(dataContext);
			var requests = dataContext.FollowRequests;

			Assert.Equal(12, requests.Count);
			Assert.Contains(requests, r => r.RequestUri.ToString() == "http://cnblogs.com/sitehome/p/2");
		}

		/// <summary>
		/// 通过正则判断 URL 是否需要当前 DataParser 处理
		/// </summary>
		[Fact(DisplayName = "RegexCanParse")]
		public async Task RequiredValidator()
		{
			var request = new Request("http://cnblogs.com");
			var dataContext =
				new DataFlowContext(null, new SpiderOptions(), request,
					new Response {Content = new ByteArrayContent(File.ReadAllBytes("cnblogs.html"))});

			var dataParser = new TestDataParser();
			dataParser.SetLogger(NullLogger.Instance);
			dataParser.AddFollowRequestQuerier(Selectors.XPath(".//div[@class='pager']"));
			dataParser.AddRequiredValidator(r => Regex.IsMatch(r.RequestUri.ToString(), "xxxcnblogs\\.com"));

			await dataParser.HandleAsync(dataContext);
			var requests = dataContext.FollowRequests;

			Assert.Empty(requests);

			var dataContext2 =
				new DataFlowContext(null, new SpiderOptions(), request,
					new Response {Content = new ByteArrayContent(File.ReadAllBytes("cnblogs.html"))});
			var dataParser2 = new TestDataParser();
			dataParser2.AddFollowRequestQuerier(Selectors.XPath(".//div[@class='pager']"));
			dataParser.AddRequiredValidator(r => Regex.IsMatch(r.RequestUri.ToString(), "cnblogs\\.com"));

			await dataParser2.HandleAsync(dataContext2);
			requests = dataContext2.FollowRequests;

			Assert.Equal(12, requests.Count);
			Assert.Contains(requests, r => r.RequestUri.ToString() == "http://cnblogs.com/sitehome/p/2");
		}

		class TestDataParser : DataParser
		{
			protected override Task ParseAsync(DataFlowContext context)
			{
				return Task.CompletedTask;
			}

			public override Task InitializeAsync()
			{
				return Task.CompletedTask;
			}
		}
	}
}
