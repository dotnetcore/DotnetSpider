using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Sample.samples
{
	public class GithubSpider : Spider
	{
		class Parser : DataParserBase
		{
			protected override Task<DataFlowResult> Parse(DataFlowContext context)
			{
				var selectable = context.GetSelectable();
				// 解析数据
				var author = selectable.XPath("//span[@class='p-name vcard-fullname d-block overflow-hidden']")
					.GetValue();
				var name = selectable.XPath("//span[@class='p-nickname vcard-username d-block']")
					.GetValue();
				context.AddItem("author", author);
				context.AddItem("username", name);

				// 添加目标链接
				var urls = selectable.Links().Regex("(https://github\\.com/[\\w\\-]+/[\\w\\-]+)").GetValues();
				AddFollowRequests(context, urls);

				// 如果解析为空，跳过后续步骤(存储 etc)
				if (string.IsNullOrWhiteSpace(name))
				{
					context.ClearItems();
					return Task.FromResult(DataFlowResult.Terminated);
				}

				return Task.FromResult(DataFlowResult.Success);
			}
		}

		public GithubSpider(IEventBus mq, IStatisticsService statisticsService, SpiderOptions options,
			ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
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