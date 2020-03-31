using System.IO;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Http;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests
{
    public partial class DataParserTests : TestBase
    {
        /// <summary>
        /// 从 HTML 指定的 XPATH 元素下面查找所有的 URL
        /// </summary>
        [Fact()]
        public async Task XpathFollow()
        {
            var request = new Request("http://cnblogs.com");
            var dataContext =
                new DataContext(null, new SpiderOptions(), request,
                    new Response
                    {
                        Content = new ResponseContent
                        {
                            Data = File.ReadAllBytes("cnblogs.html")
                        }
                    });


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
        public void RegexCanParse()
        {
            // TODO
        }

        class TestDataParser : DataParser
        {
            protected override Task Parse(DataContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}