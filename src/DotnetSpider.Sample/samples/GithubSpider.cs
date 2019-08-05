using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.samples
{
	public class GithubSpider : Spider
	{
		class Parser : DataParserBase
		{
			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				var selectable = context.Selectable;
				// 解析数据
				var author = selectable.XPath("//span[@class='p-name vcard-fullname d-block overflow-hidden']")
					.GetValue();
				var name = selectable.XPath("//span[@class='p-nickname vcard-username d-block']")
					.GetValue();
				context.AddData("author", author);
				context.AddData("username", name);

				// 添加目标链接
				var urls = selectable.Links().Regex("(https://github\\.com/[\\w\\-]+/[\\w\\-]+)").GetValues();
				foreach (var url in urls)
				{
					context.AddExtraRequests(CreateFromRequest(context.Response.Request, url));
				}


				// 如果解析为空，跳过后续步骤(存储 etc)
				if (string.IsNullOrWhiteSpace(name))
				{
					context.ClearData();
					return Task.FromResult(DataFlowResult.Terminated);
				}

				return Task.FromResult(DataFlowResult.Success);
			}
		}

		public GithubSpider(SpiderParameters parameters) : base(parameters)
		{
		}

		protected override void Initialize()
		{
			NewGuidId();
			AddDataFlow(new Parser()).AddDataFlow(new ConsoleStorage());
			AddRequests(new Request("https://github.com/zlzforever"));
		}
	}
}
