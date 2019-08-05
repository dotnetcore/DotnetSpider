using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Selector;

namespace DotnetSpider.Sample.samples
{
	public class CnblogsSpider : Spider
	{
		protected override void Initialize()
		{
			NewGuidId();
			Depth = 3;
			AddDataFlow(new ListNewsParser()).AddDataFlow(new NewsParser()).AddDataFlow(new ConsoleStorage());
			AddRequests("https://news.cnblogs.com/n/page/1/");
		}

		class ListNewsParser : DataParser
		{
			public ListNewsParser()
			{
				Required = DataParserHelper.CheckIfRequiredByRegex("news\\.cnblogs\\.com/n/page");
				// 如果你还想翻页则可以去掉注释
				//FollowRequestQuerier =
				//	BuildFollowRequestQuerier(DataParserHelper.QueryFollowRequestsByXPath(".//div[@class='pager']"));
			}

			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				var news = context.Selectable.XPath(".//div[@class='news_block']").Nodes();
				foreach (var item in news)
				{
					var title = item.Select(Selectors.XPath(".//h2[@class='news_entry']"))
						.GetValue(ValueOption.InnerText);
					var url = item.Select(Selectors.XPath(".//h2[@class='news_entry']/a/@href")).GetValue();
					var summary = item.Select(Selectors.XPath(".//div[@class='entry_summary']"))
						.GetValue(ValueOption.InnerText);
					var views = item.Select(Selectors.XPath(".//span[@class='view']")).GetValue(ValueOption.InnerText)
						.Replace(" 人浏览", "");
					var request = CreateFromRequest(context.Response.Request, url);
					request.AddProperty("title", title);
					request.AddProperty("summary", summary);
					request.AddProperty("views", views);

					context.AddExtraRequests(request);
				}

				return Task.FromResult(DataFlowResult.Success);
			}
		}

		class NewsParser : DataParser
		{
			public NewsParser()
			{
				Required = DataParserHelper.CheckIfRequiredByRegex("news\\.cnblogs\\.com/n/\\d+");
			}

			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				var typeName = typeof(News).FullName;
				context.AddData(typeName,
					new News
					{
						Url = context.Response.Request.Url,
						Title = context.Response.Request.Properties["title"],
						Summary = context.Response.Request.Properties["summary"],
						Views = int.Parse(context.Response.Request.Properties["views"]),
						Content = context.Selectable.Select(Selectors.XPath(".//div[@id='news_body']")).GetValue()
					});
				return Task.FromResult(DataFlowResult.Success);
			}
		}

		public CnblogsSpider(SpiderParameters parameters) : base(parameters)
		{
		}

		class News
		{
			public string Title { get; set; }
			public string Url { get; set; }
			public string Summary { get; set; }
			public int Views { get; set; }

			public string Content { get; set; }
		}
	}
}
