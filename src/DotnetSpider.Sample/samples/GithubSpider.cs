using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Sample.samples
{
	public class GithubSpider : Spider
	{
		public GithubSpider(IOptions<SpiderOptions> options, DependenceServices services, ILogger<Spider> logger) :
			base(options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			// 添加自定义解析
			AddDataFlow(new Parser());
			// 使用控制台存储器
			AddDataFlow(new ConsoleStorage());
			// 添加采集请求
			await AddRequestsAsync(new Request("https://github.com/zlzforever")
			{
				// 请求超时 10 秒
				Timeout = 10000
			});
		}

		protected override SpiderId GenerateSpiderId()
		{
			return new(ObjectId.CreateId().ToString(), "Github");
		}

		class Parser : DataParser
		{
			public override Task InitializeAsync()
			{
				return Task.CompletedTask;
			}

			protected override Task ParseAsync(DataFlowContext context)
			{
				var selectable = context.Selectable;
				// 解析数据
				var author = selectable.XPath("//span[@class='p-name vcard-fullname d-block overflow-hidden']")
					?.Value;
				var name = selectable.XPath("//span[@class='p-nickname vcard-username d-block']")
					?.Value;
				context.AddData("author", author);
				context.AddData("username", name);
				return Task.CompletedTask;
			}
		}
	}
}
